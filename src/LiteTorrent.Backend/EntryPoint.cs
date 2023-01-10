using System.Net;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace LiteTorrent.Backend;

public static class EntryPoint
{
    public static async Task Main(string[] args)
    {
        var port = args.Length < 1 ? 3000 : int.Parse(args[0]);
        
        var host = WebHost.CreateDefaultBuilder()
            .UseKestrel(options => options.Listen(IPAddress.Loopback, port))
            .UseStartup<Startup>()
            .Build();
        
        await host.RunAsync();
    }
}