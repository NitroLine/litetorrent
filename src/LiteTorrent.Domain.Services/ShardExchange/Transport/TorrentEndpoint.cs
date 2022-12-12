using System.Net.Sockets;
using System.Net.WebSockets;
using LiteTorrent.Domain.Services.LocalStorage.SharedFiles;
using LiteTorrent.Domain.Services.ShardExchange.Messages;
using LiteTorrent.Domain.Services.ShardExchange.Serialization;

namespace LiteTorrent.Domain.Services.ShardExchange.Transport;

public class TorrentEndpoint
{
    private readonly SharedFileRepository sharedFileRepository;
    private readonly Socket socketEndpoint;

    public TorrentEndpoint(
        TransportConfiguration transportConfiguration,
        SharedFileRepository sharedFileRepository)
    {
        this.sharedFileRepository = sharedFileRepository;
        socketEndpoint = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socketEndpoint.Bind(transportConfiguration.TorrentEndpoint);
        socketEndpoint.Listen();
    }
    
    public async Task<Peer> Accept(string peerId, CancellationToken cancellationToken)
    {
        var socket = await socketEndpoint.AcceptAsync(cancellationToken);
        var ws = WebSocket.CreateFromStream(new NetworkStream(socket), new WebSocketCreationOptions {IsServer = true});
        
        var buffer = new byte[65536];
        var result = await ws.ReceiveAsync(buffer, cancellationToken);
        var initMessage = (HandshakeInitMessage)MessageSerializer.Deserialize(buffer.AsMemory()[..result.Count]);
        
        var getResult = await sharedFileRepository.Get(initMessage.FileHash, cancellationToken);
        if (getResult.TryGetError(out var sharedFile, out var error))
            throw new Exception(error.Message); // TODO: custom exceptions

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

        return new Peer(initMessage.PeerId, ws, new ConnectionContext(sharedFile, bitfield.Mask));
    }
}