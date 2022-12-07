using System.Net.Sockets;
using System.Net.WebSockets;
using LiteTorrent.Domain.Services.ShardExchange.Messages;
using LiteTorrent.Domain.Services.ShardExchange.Serialization;

namespace LiteTorrent.Domain.Services.ShardExchange.Transport;

public class TorrentEndpoint
{
    private readonly Socket Server = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    
    public async Task<Peer> Accept(CancellationToken cancellationToken)
    {
        var socket = await Server.AcceptAsync(cancellationToken);
        var ws = WebSocket.CreateFromStream(new NetworkStream(socket), new WebSocketCreationOptions {IsServer = true});
        
        var buffer = new Memory<byte>(new byte[1024]);
        var result = await ws.ReceiveAsync(buffer, cancellationToken);
        var init = (HandshakeInitMessage)MessageSerializer.Deserialize(buffer[..result.Count]);
        await ws.SendAsync(
            MessageSerializer.Serialize(new HandshakeAckMessage("", new Hash())),
            WebSocketMessageType.Binary, 
            true,
            cancellationToken);

        return new Peer(init.PeerId, ws, new ConnectionContext(init.FileHash));
    }
}