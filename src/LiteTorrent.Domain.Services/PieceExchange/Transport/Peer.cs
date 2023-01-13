using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using LiteTorrent.Core;
using LiteTorrent.Domain.Services.Common;
using LiteTorrent.Domain.Services.PieceExchange.Messages;
using LiteTorrent.Domain.Services.PieceExchange.Serialization;

namespace LiteTorrent.Domain.Services.PieceExchange.Transport;

public class Peer
{
    // ReSharper disable once NotAccessedField.Local
    private readonly string id;
    private readonly WebSocket webSocket;
    private readonly byte[] buffer;

    public Peer(string id, WebSocket webSocket, ConnectionContext context)
    {
        this.id = id;
        this.webSocket = webSocket;
        Context = context;

        var bufferSize = 2 * (context.SharedFile.PieceMaxSizeInBytes 
                              + 32 * (int)Math.Log2(context.OtherBitfield.Count)); 
        buffer = new byte[bufferSize];
    }
    
    public bool IsClosed { get; private set; }
    
    public ConnectionContext Context { get; }

    public Task Send(object message, CancellationToken cancellationToken)
    {
        if (IsClosed)
            throw new InvalidOperationException();
        
        return webSocket.SendAsync(
            MessageSerializer.Serialize(message),
            WebSocketMessageType.Binary,
            true,
            cancellationToken);
    }

    public async IAsyncEnumerable<Result<object>> Receive([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (IsClosed)
                throw new InvalidOperationException();
        
            var receiveResult = await webSocket.ReceiveAsync(buffer, cancellationToken);
            if (receiveResult.MessageType == WebSocketMessageType.Close)
            {
                await Close(cancellationToken);
            
                yield return ErrorRegistry.Peer.ConnectionIsClosed();
                yield break;
            }

            yield return MessageSerializer.Deserialize(buffer);
        
            Array.Fill<byte>(buffer, 0);
        }
    }

    public Task Close(CancellationToken cancellationToken)
    {
        IsClosed = true;
        
        return webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, cancellationToken);
    }
}