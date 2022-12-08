using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using LiteTorrent.Core;
using LiteTorrent.Domain.Services.Common;
using LiteTorrent.Domain.Services.ShardExchange.Messages;
using LiteTorrent.Domain.Services.ShardExchange.Serialization;

namespace LiteTorrent.Domain.Services.ShardExchange.Transport;

public class Peer
{
    private readonly string id;
    private readonly WebSocket webSocket;
    private readonly byte[] buffer = new byte[40960];

    public Peer(string id, WebSocket webSocket, ConnectionContext context)
    {
        this.id = id;
        this.webSocket = webSocket;
        Context = context;
    }
    
    public ConnectionContext Context { get; }

    public Task Send(object message, CancellationToken cancellationToken)
    {
        return webSocket.SendAsync(
            MessageSerializer.Serialize(message),
            WebSocketMessageType.Binary,
            true,
            cancellationToken);
    }

    public async IAsyncEnumerable<Result<object>> Receive([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var receiveResult = await webSocket.ReceiveAsync(buffer, cancellationToken);
        if (receiveResult.MessageType == WebSocketMessageType.Close)
        {
            yield return ErrorRegistry.Peer.ConnectionIsClosed();
            yield break;
        }

        yield return MessageSerializer.Deserialize(buffer);
        
        Array.Fill<byte>(buffer, 0);
    }

    public Task Close(CancellationToken cancellationToken)
    {
        return webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, cancellationToken);
    }
}