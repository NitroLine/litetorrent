using System.Net;
using LiteTorrent.Core;
using LiteTorrent.Domain.Services.LocalStorage.Configuration;
using LiteTorrent.Domain.Services.PieceExchange.Transport;
using Microsoft.Extensions.Configuration;

namespace LiteTorrent.Domain.Services.Common.Serialization;

public class ConfigurationParser
{
    private readonly IConfiguration configuration;

    public ConfigurationParser(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public LocalStorageConfiguration GetLocalStorageConfiguration()
    {
        const string prefix = "LocalStorage";

        var config = new LocalStorageConfiguration(
            configuration[$"{prefix}:ShardDirectoryPath"] ?? throw new ConfigurationParsingException(),
            configuration[$"{prefix}:HashTreeDirectoryPath"] ?? throw new ConfigurationParsingException(),
            configuration[$"{prefix}:SharedFileDirectoryPath"] ?? throw new ConfigurationParsingException());

        Directory.CreateDirectory(config.ShardDirectoryPath);
        Directory.CreateDirectory(config.HashTreeDirectoryPath);
        Directory.CreateDirectory(config.SharedFileDirectoryPath);

        return config;
    }

    public TransportConfiguration GetTransportConfiguration()
    {
        const string prefix = "Transport";

        var config = new TransportConfiguration(
            ParseIpEndPoint(configuration[$"{prefix}:TorrentEndpoint"] ?? throw new ConfigurationParsingException()),
            configuration[$"{prefix}:PeerId"] ?? throw new ConfigurationParsingException());

        return config;
    }

    private static IPEndPoint ParseIpEndPoint(string address)
    {
        return IPEndPoint.Parse(address);
    }
}