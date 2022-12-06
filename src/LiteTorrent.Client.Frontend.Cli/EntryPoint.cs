using System.IO.Pipes;
using System.Text;

namespace LiteTorrent.Client.Frontend.Cli;

public static class EntryPoint
{
    public static async Task Main()
    {
        // TODO: creating torrent file
        // TODO: loading existing torrent file
        
        // lt --help
        // lt start -> start backend daemon
        // lt stop -> stop backend daemon
        // lt create <raw file relative path> -> creating torrent file and return it
        // lt add <torrent file> -> add torrent file to distribution

        await using var client = new NamedPipeClientStream("LiteTorrentServer");
        Console.WriteLine("Try to connect");
        await client.ConnectAsync();
        Console.WriteLine("Connected");

        var cancelSource = new CancellationTokenSource();
        Console.CancelKeyPress += (_, _) => cancelSource.Cancel(); 
        
        while (!cancelSource.Token.IsCancellationRequested)
        {
            var message = Console.ReadLine() ?? "Some data";
            await client.WriteAsync(Encoding.UTF8.GetBytes(message), cancelSource.Token);
            Console.WriteLine($"Sent: {message}");
            
            var buffer = new Memory<byte>(new byte[1024]);
            var count = await client.ReadAsync(buffer, cancelSource.Token);
            Console.WriteLine($"Received: {Encoding.UTF8.GetString(buffer[..count].Span)}");
        }
    }
}