using System.Net;
using LiteTorrent.Core;
using LiteTorrent.Domain.Services.Common;
using LiteTorrent.Domain.Services.LocalStorage.HashTrees;
using LiteTorrent.Domain.Services.LocalStorage.SharedFiles;
using LiteTorrent.Domain.Services.PieceExchange.Messages;
using LiteTorrent.Domain.Services.PieceExchange.Transport;
using Microsoft.Extensions.Logging;

namespace LiteTorrent.Domain.Services.PieceExchange;

public class PieceExchanger
{
    private readonly string peerId;
    private readonly TorrentServer server;
    private readonly TorrentConnector connector;
    private readonly HandlerResolver handlerResolver;
    private readonly SharedFileRepository sharedFileRepository;
    private readonly HashTreeRepository hashTreeRepository;
    private readonly ILogger<PieceExchanger> logger;

    public PieceExchanger(
        TorrentServer server,
        TorrentConnector connector,
        HandlerResolver handlerResolver,
        SharedFileRepository sharedFileRepository,
        HashTreeRepository hashTreeRepository,
        ILogger<PieceExchanger> logger)
    {
        peerId = Guid.NewGuid().ToString();
        
        this.server = server;
        this.connector = connector;
        this.handlerResolver = handlerResolver;
        this.sharedFileRepository = sharedFileRepository;
        this.hashTreeRepository = hashTreeRepository;
        this.logger = logger;
    }
    
    public async Task StartDistributing(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested) 
        {
            var peer = await server.Accept(peerId, cancellationToken);
    #pragma warning disable CS4014
            ExceptionHelper.HandleException(StartReceiving(peer, cancellationToken), logger);
    #pragma warning restore CS4014
            await hashTreeRepository.CreateOrReplace(peer.Context.SharedFile.HashTree);
        }
    }

    /// <summary>
    /// Try to download file from given hosts.  
    /// </summary>
    /// <returns>
    /// If all file pieces were downloaded it returns Result.Ok else it returns Result with error 
    /// </returns>
    public async Task<Result<Unit>> TryDownload(
        DnsEndPoint[] hosts,
        SharedFile sharedFile,
        CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            foreach (var host in hosts)
            {
                logger.LogInformation("Try to connect to {host}", host);
                
                var peer = await connector.Connect(sharedFile, host, cancellationToken);
#pragma warning disable CS4014
                await ExceptionHelper.HandleException(
#pragma warning restore CS4014
                    HandleDownloadingPeer(peer, cancellationToken),
                    logger);
                    
                await hashTreeRepository.CreateOrReplace(peer.Context.SharedFile.HashTree);

                await peer.Close(cancellationToken);
            }
        }

        return Result.Ok;
    }

    private async Task HandleDownloadingPeer(Peer peer, CancellationToken cancellationToken)
    {
#pragma warning disable CS4014
        StartReceiving(peer, cancellationToken);
#pragma warning restore CS4014
        var requiredShards = peer.Context.SharedFile.HashTree.GetLeafStates();
        for (var i = 0; i < requiredShards.Count; i++)
        {
            if (!requiredShards.Get(i))
                continue;

            await peer.Send(new PieceRequestMessage((ulong)i), cancellationToken);
        }
    }

    private async Task StartReceiving(Peer peer, CancellationToken cancellationToken)
    {
        await foreach (var receiveResult in peer.Receive(cancellationToken))
        {
            if (receiveResult.TryGetError(out var message, out var error))
                throw new InvalidOperationException(error.Message);

            var handler = handlerResolver.Resolve(message);
            var handleResult = await handler.Handle(peer.Context, message, cancellationToken);
            if (!handleResult.IsNeedToSend)
                continue;

            await peer.Send(handleResult.Payload, cancellationToken);
        }
    }
}