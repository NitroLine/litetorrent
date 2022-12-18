using System.Net;
using LiteTorrent.Core;
using LiteTorrent.Domain.Services.LocalStorage.Configuration;
using LiteTorrent.Domain.Services.PieceExchange;
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
            ParseIPEndPoint(configuration[$"{prefix}:TorrentEndpoint"] ?? throw new ConfigurationParsingException()),
            configuration[$"{prefix}:PeerId"] ?? throw new ConfigurationParsingException());

        return config;
    }

    public DownloadingConfiguration GetDownloadingConfiguration()
    {
        const string prefix = "Downloading";
        var names = configuration.GetSection($"{prefix}:AllowedPeers").Get<string[]>()
                    ?? throw new ConfigurationParsingException();

        if (names.Length < 1)
            throw new ConfigurationParsingException();

        var config = new DownloadingConfiguration(names.Select(ParseDnsEndPoint).ToArray());

        return config;
    }

    private static DnsEndPoint ParseDnsEndPoint(string address)
    {
        var splitAddress = address.Split(':');
        return new DnsEndPoint(splitAddress[0], int.Parse(splitAddress[1]));
    }

    private static IPEndPoint ParseIPEndPoint(string address)
    {
        return IPEndPoint.Parse(address);
    }
}