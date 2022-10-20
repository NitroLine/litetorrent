using System.Net;

namespace NatPuncher.EchoServer;

public static class EntryPoint
{
    public static async Task Main()
    {
        var server = new EchoServer();

        var cancellationTokenSource = new CancellationTokenSource();
        var serverTask = server.Run(new IPEndPoint(IPAddress.Parse("0.0.0.0"), 7000), cancellationTokenSource.Token);

        await Console.In.ReadLineAsync();
        cancellationTokenSource.Cancel();

        await serverTask;
    }
}