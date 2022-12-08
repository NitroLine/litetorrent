using LiteTorrent.Domain.Services.Common.Serialization;
using LiteTorrent.Domain.Services.LocalStorage.HashTrees;
using LiteTorrent.Domain.Services.LocalStorage.Shards;
using LiteTorrent.Domain.Services.LocalStorage.SharedFiles;
using LiteTorrent.Domain.Services.ShardExchange;
using LiteTorrent.Domain.Services.ShardExchange.Transport;
using MessagePipe;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LiteTorrent.Client.Cli;

public static class EntryPoint
{
    public static async Task Main()
    {
        // TODO: creating torrent file
        // TODO: loading existing torrent file
        
        // lt --help
        // lt start -> start interactive mode
        // create <raw file relative path> -> creating torrent file and return it
        // add <torrent file path> -> add torrent file to distribution
        
        // TODO: Services: CLI, Server (distributing)

        var commonConfig = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        var configParser = new ConfigurationParser(commonConfig);
        var localStorageConfiguration = configParser.GetLocalStorageConfiguration();
        var transportConfiguration = configParser.GetTransportConfiguration();
        var downloadingConfiguration = configParser.GetDownloadingConfiguration();

        var host = Host
            .CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services
                    .AddMessagePipe();
                
                services
                    .AddSingleton(localStorageConfiguration)
                    .AddSingleton(transportConfiguration)
                    .AddSingleton(downloadingConfiguration)
                    .AddSingleton<SharedFileRepository>()
                    .AddSingleton<ShardRepository>()
                    .AddSingleton<HashTreeRepository>()
                    .AddSingleton<TorrentEndpoint>()
                    .AddSingleton<TorrentConnector>()
                    .AddSingleton<ShardExchanger>()
                    .AddLogging();
                    
                services
                    .AddHostedService<CliService>();
            })
            .Build();

        var cancelSource = new CancellationTokenSource();
        Console.CancelKeyPress += (_, _) => cancelSource.Cancel();

        await host.RunAsync(cancelSource.Token);
    }
}