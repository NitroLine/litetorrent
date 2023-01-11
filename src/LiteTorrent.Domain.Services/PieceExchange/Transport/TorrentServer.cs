using System.Net.Sockets;
using System.Net.WebSockets;
using LiteTorrent.Core;
using LiteTorrent.Domain.Services.Common;
using LiteTorrent.Domain.Services.LocalStorage.SharedFiles;
using LiteTorrent.Domain.Services.PieceExchange.Messages;
using LiteTorrent.Domain.Services.PieceExchange.Serialization;
using Microsoft.Extensions.Logging;

namespace LiteTorrent.Domain.Services.PieceExchange.Transport;

public class TorrentServer
{
    private readonly SharedFileRepository sharedFileRepository;
    private readonly ILogger<TorrentServer> logger;
    private readonly Socket socketEndpoint;

    public TorrentServer(
        TransportConfiguration transportConfiguration,
        SharedFileRepository sharedFileRepository,
        ILogger<TorrentServer> logger)
    {
        this.sharedFileRepository = sharedFileRepository;
        this.logger = logger;
        
        socketEndpoint = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socketEndpoint.Bind(transportConfiguration.TorrentEndpoint);
        socketEndpoint.Listen();
        
        logger.LogInformation("Torrent Distributing was started at {ip}", transportConfiguration.TorrentEndpoint);
    }

    public async Task<Peer> Accept(string peerId, Hash? excludeHash, CancellationToken cancellationToken)
    {
        var peer = (Peer?)null;
        while (!cancellationToken.IsCancellationRequested)
        {
            var result = await TryAccept(peerId, excludeHash, cancellationToken);
            if (!result.TryGetError(out peer, out var error))
                break;
            
            logger.LogWarning(error.Message);
        }

        return peer!;
    }
    
    private async Task<Result<Peer>> TryAccept(string peerId, Hash? excludeHash, CancellationToken cancellationToken)
    {
        var socket = await socketEndpoint.AcceptAsync(cancellationToken);
        var ws = WebSocket.CreateFromStream(new NetworkStream(socket), new WebSocketCreationOptions {IsServer = true});
        
        logger.LogInformation("Incoming connection {ip}", socket.RemoteEndPoint);
        
        var buffer = new byte[65536];
        var result = await ws.ReceiveAsync(buffer, cancellationToken);
        var initMessage = (HandshakeInitMessage)MessageSerializer.Deserialize(buffer.AsMemory()[..result.Count]);

        if (excludeHash is not null && initMessage.FileHash == excludeHash)
            return ErrorRegistry.Peer.RequestedFileIsDownloading();

        var getResult = await sharedFileRepository.Get(initMessage.FileHash, cancellationToken);
        if (getResult.TryGetError(out var sharedFile, out var error))
            return error; // TODO: custom exceptions

        await ws.SendAsync(
            MessageSerializer.Serialize(new HandshakeAckMessage(peerId, sharedFile.Hash)),
            WebSocketMessageType.Binary,
            true,
            cancellationToken);

        await ws.SendAsync(
            MessageSerializer.Serialize(new BitfieldMessage(sharedFile.HashTree.GetLeafStates())),
            WebSocketMessageType.Binary,
            true,
            cancellationToken);
        
        Array.Fill(buffer, (byte)0);
        result = await ws.ReceiveAsync(buffer, cancellationToken);
        var bitfield = (BitfieldMessage)MessageSerializer.Deserialize(buffer.AsMemory()[..result.Count]);
        
        logger.LogInformation("Connection was established [{ip}]", socket.RemoteEndPoint);

        return new Peer(initMessage.PeerId, ws, new ConnectionContext(sharedFile, bitfield.Mask));
    }
}