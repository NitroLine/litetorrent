using System.Net;
using System.Net.Sockets;

namespace Tracker;

public static class EntryPoint
{
    private static readonly List<Socket> Sockets = new();

    public static async Task Main()
    {
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        socket.Bind(new IPEndPoint(IPAddress.Parse("0.0.0.0"), 500));
        socket.Listen();

        var connectedSockets = new List<Socket>();

        while (!cancellationToken.IsCancellationRequested)
        {
            var connectedSocket = await socket.AcceptAsync(cancellationToken);
            connectedSocket.
        }
    }

    private static async Task ServeConnection(Socket connection)
    {
        
    }
}