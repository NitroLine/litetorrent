using System.Net;
using LiteTorrent.Domain.Services.Common;
using LiteTorrent.Domain.Services.LocalStorage.HashTrees;
using LiteTorrent.Domain.Services.LocalStorage.SharedFiles;
using LiteTorrent.Domain.Services.ShardExchange.Messages;
using LiteTorrent.Domain.Services.ShardExchange.Transport;
using Microsoft.Extensions.Logging;

namespace LiteTorrent.Domain.Services.ShardExchange;

public class ShardExchanger
{
    private readonly TorrentEndpoint endpoint;
    private readonly TorrentConnector connector;
    private readonly HandlerResolver handlerResolver;
    private readonly DownloadingConfiguration downloadingConfiguration;
    private readonly SharedFileRepository sharedFileRepository;
    private readonly HashTreeRepository hashTreeRepository;
    private readonly ILogger<ShardExchanger> logger;

    public ShardExchanger(
        TorrentEndpoint endpoint,
        TorrentConnector connector,
        HandlerResolver handlerResolver, 
        DownloadingConfiguration downloadingConfiguration,
        SharedFileRepository sharedFileRepository,
        HashTreeRepository hashTreeRepository,
        ILogger<ShardExchanger> logger)
    {
        this.endpoint = endpoint;
        this.connector = connector;
        this.handlerResolver = handlerResolver;
        this.downloadingConfiguration = downloadingConfiguration;
        this.sharedFileRepository = sharedFileRepository;
        this.hashTreeRepository = hashTreeRepository;
        this.logger = logger;
    }
    
    public async Task StartDistributing(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested) 
        {
            var peer = await endpoint.Accept("peer", cancellationToken);
    #pragma warning disable CS4014
            ExceptionHelper.HandleException(StartReceiving(peer, cancellationToken), logger);
    #pragma warning restore CS4014
            await hashTreeRepository.CreateOrReplace(peer.Context.SharedFile.HashTree);
        }
    }

    public async Task StartDownloading(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var getAllResult = await sharedFileRepository.GetAll(cancellationToken);
            if (getAllResult.TryGetError(out var sharedFiles, out var error))
                throw new Exception(error.Message);
            
            foreach (var sharedFile in sharedFiles)
            {
                var hosts = await GetHosts(sharedFile.Hash, cancellationToken);
                foreach (var host in hosts)
                {
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
        }
    }

    private Task<DnsEndPoint[]> GetHosts(Hash hash, CancellationToken cancellationToken)
    {
        return Task.FromResult(downloadingConfiguration.AllowedPeers);
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

            await peer.Send(new ShardRequestMessage((ulong)i), cancellationToken);
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