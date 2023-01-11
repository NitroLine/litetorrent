using System.Net;
using LiteTorrent.Core;
using LiteTorrent.Domain.Services.Common;
using LiteTorrent.Domain.Services.LocalStorage.HashTrees;
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
    private readonly HashTreeRepository hashTreeRepository;
    private readonly ILogger<PieceExchanger> logger;

    private CancellationTokenSource? source;
    private Task<Result<Unit>>? currentDownloading;
    private Hash? downloadingFileHash;

    public PieceExchanger(
        TorrentServer server,
        TorrentConnector connector,
        HandlerResolver handlerResolver,
        HashTreeRepository hashTreeRepository,
        ILogger<PieceExchanger> logger)
    {
        peerId = Guid.NewGuid().ToString();
        
        this.server = server;
        this.connector = connector;
        this.handlerResolver = handlerResolver;
        this.hashTreeRepository = hashTreeRepository;
        this.logger = logger;
    }
    
    public async Task StartDistributing(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested) 
        {
            var peer = await server.Accept(peerId, downloadingFileHash, cancellationToken);
#pragma warning disable CS4014
            ExceptionHelper.HandleException(HandleReceivedMessages(peer, cancellationToken), logger);
#pragma warning restore CS4014
            // await hashTreeRepository.CreateOrReplace(peer.Context.SharedFile.HashTree);
        }
    }

    public Task<Hash?> GetDownloadingFile()
    {
        return Task.FromResult(downloadingFileHash);
    }

    public async Task StartDownloading(
        IEnumerable<IPEndPoint> hosts,
        SharedFile sharedFile,
        CancellationToken cancellationToken)
    {
        if (currentDownloading is not null)
        {
            source!.Cancel();
            
            logger.LogDebug("Trying to stop current downloading");
            await currentDownloading!;
        }
        
        source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        currentDownloading = TryDownload(hosts, sharedFile, source.Token);
        downloadingFileHash = sharedFile.Hash;
    } 

    /// <summary>
    /// Try to download file from given hosts.  
    /// </summary>
    /// <returns>
    /// If all file pieces were downloaded it returns Result.Ok else it returns Result with error 
    /// </returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public async Task<Result<Unit>> TryDownload(
        IEnumerable<IPEndPoint> hosts,
        SharedFile sharedFile,
        CancellationToken cancellationToken)
    {
        foreach (var host in hosts)
        {
            if (cancellationToken.IsCancellationRequested)
                break;
            
            logger.LogWarning("Try to connect to {host}", host);

            // ReSharper disable once RedundantAssignment
            var peer = (Peer)null!;
            try
            {
                peer = await connector.Connect(sharedFile, host, cancellationToken);
            }
            catch (TimeoutException)
            {
                logger.LogWarning("Connection attempt to {host} is failed", host);
                continue;
            }

            await ExceptionHelper.HandleException(SendPieceRequests(peer, cancellationToken), logger);
                    
            await hashTreeRepository.CreateOrReplace(peer.Context.SharedFile.HashTree);
        }
            
        source = null;
        currentDownloading = null;
        downloadingFileHash = null;

        return Result.Ok;
    }

    private async Task SendPieceRequests(Peer peer, CancellationToken cancellationToken)
    {
        var requiredShards = peer.Context.SharedFile.HashTree.GetLeafStates();
        for (var i = 0; i < requiredShards.Count; i++)
        {
            if (requiredShards.Get(i))
                continue;
           
            logger.LogWarning("Try to request piece {index}", i);
            
            await peer.Send(new PieceRequestMessage((ulong)i), cancellationToken);
        }

        var receiveEnumerable = peer
            .Receive(cancellationToken)
            .Take(requiredShards.Count)
            .WithCancellation(cancellationToken);

        await foreach (var receiveResult in receiveEnumerable)
            await HandleReceivedMessage(peer, receiveResult, cancellationToken);

        if (!peer.IsClosed)
        {
            logger.LogWarning("Stop downloading {hash}", peer.Context.SharedFile.Hash);
            await peer.Close(cancellationToken);
        }
    }

    private async Task HandleReceivedMessages(Peer peer, CancellationToken cancellationToken)
    {
        await foreach (var receiveResult in peer.Receive(cancellationToken)) 
            await HandleReceivedMessage(peer, receiveResult, cancellationToken);
    }

    private async Task HandleReceivedMessage(
        Peer peer,
        Result<object> receiveResult,
        CancellationToken cancellationToken)
    {
        if (receiveResult.TryGetError(out var message, out var error))
        {
            logger.LogWarning(error.Message);
            return;
        }

        var handler = handlerResolver.Resolve(message);
        var handleResult = await handler.Handle(peer.Context, message, cancellationToken);
        if (!handleResult.IsNeedToSend)
            return;

        await peer.Send(handleResult.Payload, cancellationToken);
    }
}