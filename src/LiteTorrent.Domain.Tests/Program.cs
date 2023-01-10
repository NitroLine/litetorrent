using LiteTorrent.Domain.Services.Common.Serialization;
using LiteTorrent.Domain.Services.LocalStorage.HashTrees;
using LiteTorrent.Domain.Services.LocalStorage.Pieces;
using LiteTorrent.Domain.Services.LocalStorage.SharedFiles;
using MessagePipe;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LiteTorrent.Domain.Tests;

public static class Program
{
    public static async Task Main()
    {
        var services = new ServiceCollection();
        var commonConfig = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
        
        var configParser = new ConfigurationParser(commonConfig);
        var localStorageConfiguration = configParser.GetLocalStorageConfiguration();
        var transportConfiguration = configParser.GetTransportConfiguration();
        
        services.AddMessagePipe();

        services
            .AddLogging()
            .AddSingleton(localStorageConfiguration)
            .AddSingleton(transportConfiguration)
            .AddSingleton<SharedFileRepository>()
            .AddSingleton<PieceRepository>()
            .AddSingleton<HashTreeRepository>();

        var provider = services.BuildServiceProvider();

        var hashTreeRepository = provider.GetService<HashTreeRepository>()!;

        var tree = new MerkleTree(new[] { Hash.CreateFromRaw(new ReadOnlyMemory<byte>(new byte[] { 0, 1, 2 })) });
        var result = await hashTreeRepository.CreateOrReplace(tree);
        
        Console.WriteLine("STOP");
        Console.ReadKey();
    }
}