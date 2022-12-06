using System.IO.Pipes;
using System.Text;

namespace LiteTorrent.Client.Backend;

public static class EntryPoint
{
    public static async Task Main()
    {
        // TODO: logging, configuration
        // var cp = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        // var lsc = new ConfigurationParser(cp).GetLocalStorageConfiguration();
        // Console.WriteLine(lsc.ShardDirectoryPath);
        // Console.WriteLine(lsc.HashTreeDirectoryPath);
        // Console.WriteLine(lsc.SharedFileDirectoryPath);
        //
        // await Host.CreateDefaultBuilder()
        //     .ConfigureServices(services => 
        //     { 
        //         services
        //             .AddSingleton(lsc)
        //             .AddHostedService<BackgroundService>();
        //     })
        //     .Build()
        //     .RunAsync();
        //
        // try
        // {
        //     throw new IOException("EXC");
        // }
        // catch (Exception e)
        // {
        //     Console.WriteLine(e.Message);
        // }

        await using var s = new NamedPipeServerStream(
            "LiteTorrentServer", 
            PipeDirection.InOut, 
            10, 
            PipeTransmissionMode.Byte,
            PipeOptions.Asynchronous,
            65000, 
            65000);
        
        await Wait(s);

        var cancelSource = new CancellationTokenSource();
        Console.CancelKeyPress += (_, _) => cancelSource.Cancel(); 
        
        while (!cancelSource.Token.IsCancellationRequested)
        {
            var buffer = new Memory<byte>(new byte[1024]);
            var count = 0;
            while (true)
            {
                try
                {
                    count = await s.ReadAsync(buffer, cancelSource.Token);
                    break;
                }
                catch (IOException e)
                {
                    Console.WriteLine(e.Message);
                    s.Disconnect();
                    await s.WaitForConnectionAsync(cancelSource.Token);
                }
            }

            Console.WriteLine($"Received: {Encoding.UTF8.GetString(buffer[..count].Span)}");

            const string answer = "Ok";
            try
            {
                await s.WriteAsync(Encoding.UTF8.GetBytes(answer), cancelSource.Token);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                s.Disconnect();
                await s.WaitForConnectionAsync(cancelSource.Token);
            }

            Console.WriteLine($"Sent: {answer}");
        }
    }

    private static async Task Wait(NamedPipeServerStream s)
    {
        await s.WaitForConnectionAsync();
        Console.WriteLine("Connection");
    }
}