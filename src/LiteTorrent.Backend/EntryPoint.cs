using System.Net;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace LiteTorrent.Backend;

public static class EntryPoint
{
    public static async Task Main()
    {
        var host = WebHost.CreateDefaultBuilder()
            .UseKestrel(options => options.Listen(IPAddress.Loopback, 3000))
            .UseStartup<Startup>()
            .Build();
        
        await host.RunAsync();
    }
}