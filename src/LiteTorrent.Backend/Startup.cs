using LiteTorrent.Domain.Services.Common.Serialization;
using LiteTorrent.Domain.Services.LocalStorage.HashTrees;
using LiteTorrent.Domain.Services.LocalStorage.Pieces;
using LiteTorrent.Domain.Services.LocalStorage.SharedFiles;
using LiteTorrent.Domain.Services.PieceExchange;
using LiteTorrent.Domain.Services.PieceExchange.Messages;
using LiteTorrent.Domain.Services.PieceExchange.Transport;
using MessagePipe;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LiteTorrent.Backend;

public class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    { 
        var commonConfig = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
        
        var configParser = new ConfigurationParser(commonConfig);
        var localStorageConfiguration = configParser.GetLocalStorageConfiguration();
        var transportConfiguration = configParser.GetTransportConfiguration();
        
        services.AddMessagePipe();

        services
            .AddSingleton(localStorageConfiguration)
            .AddSingleton(transportConfiguration)
            .AddSingleton<SharedFileRepository>()
            .AddSingleton<PieceRepository>()
            .AddSingleton<HashTreeRepository>()
            .AddSingleton<TorrentServer>()
            .AddSingleton<TorrentConnector>()
            .AddSingleton<HandlerResolver>()
            .AddSingleton<PieceRequestMessageHandler>()
            .AddSingleton<PieceResponseMessageHandler>()
            .AddSingleton<PieceExchanger>();

        services
            .AddHostedService<LiteTorrentService>();
        
        services
            .AddLogging(builder => builder.AddConsole().AddDebug()) //TODO: ADD DEBUG LEVEL LOGS
            .AddMvcCore()
            .AddJsonFormatters();
    }
    

    public override void Configure(IApplicationBuilder app)
    {
        app.UseEndpointRouting();
        app.UseEndpoint();
    }
}