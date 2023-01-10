using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using LiteTorrent.Core;
using LiteTorrent.Domain.Services.PieceExchange.Messages;
using LiteTorrent.Domain.Services.PieceExchange.Serialization;

namespace LiteTorrent.Domain.Services.PieceExchange.Transport;

public class TorrentConnector
{
    private readonly TransportConfiguration configuration;

    public TorrentConnector(TransportConfiguration configuration)
    {
        this.configuration = configuration;
    }
    
    public async Task<Peer> Connect(SharedFile sharedFile, IPEndPoint host, CancellationToken cancellationToken)
    {
        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        await socket.ConnectAsync(host, cancellationToken);
        var ws = WebSocket.CreateFromStream(new NetworkStream(socket), new WebSocketCreationOptions {IsServer = false});
        
        await ws.SendAsync(
            MessageSerializer.Serialize(new HandshakeInitMessage(configuration.PeerId, sharedFile.Hash)),
            WebSocketMessageType.Binary,
            true,
            cancellationToken);
        
        var buffer = new byte[65536];
        var result = await ws.ReceiveAsync(buffer, cancellationToken).WithTimeout();
        var ackMessage = (HandshakeAckMessage)MessageSerializer.Deserialize(buffer.AsMemory()[..result.Count]);
        
        Array.Fill(buffer, (byte)0);
        result = await ws.ReceiveAsync(buffer, cancellationToken).WithTimeout();
        var bitfield = (BitfieldMessage)MessageSerializer.Deserialize(buffer.AsMemory()[..result.Count]);
        
        await ws.SendAsync(
            MessageSerializer.Serialize(new BitfieldMessage(sharedFile.HashTree.GetLeafStates())),
            WebSocketMessageType.Binary,
            true,
            cancellationToken);
        
        return new Peer(ackMessage.PeerId, ws, new ConnectionContext(sharedFile, bitfield.Mask));
    }
}