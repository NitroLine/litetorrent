using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text.Json;

namespace NatPuncher.EchoServer;

public class EchoServer
{
    private readonly List<WebSocket> connections = new();
    
    public async Task Run(IPEndPoint bindingEndPoint, CancellationToken cancellationToken)
    {
        using var listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        listener.Bind(bindingEndPoint);
        listener.Listen();

        var listenerEndPoint = (IPEndPoint)listener.LocalEndPoint!;
        Console.WriteLine($"Server is running on {listenerEndPoint.Address}:{listenerEndPoint.Port}");
        
        while (!cancellationToken.IsCancellationRequested)
        {
            var socket = await listener.AcceptAsync(cancellationToken);
            var webSocket = WebSocket.CreateFromStream(
                new NetworkStream(socket), 
                true, 
                null, 
                TimeSpan.FromMinutes(2));

            var remoteAddress = (IPEndPoint)socket.RemoteEndPoint!;
            Console.WriteLine($"[Debug] Accepted TCP connection {remoteAddress.Address}:{remoteAddress.Port}");

            var response = CreateEchoResponse(remoteAddress);
            await webSocket.SendAsync(
                new ArraySegment<byte>(response),
                WebSocketMessageType.Binary,
                false, 
                cancellationToken);
            
            Console.WriteLine($"[Debug] Sent {response.Length} response to {remoteAddress.Address}:{remoteAddress.Port}");
            
            connections.Add(webSocket);
        }
    }

    private static byte[] CreateEchoResponse(IPEndPoint publicAddress)
    {
        var dto = new Dictionary<string, string>
        {
            ["ip"] = publicAddress.Address.ToString(),
            ["port"] = publicAddress.Port.ToString()
        };

        return JsonSerializer.SerializeToUtf8Bytes(dto);
    }
}