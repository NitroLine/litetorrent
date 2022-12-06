using LiteTorrent.Core;
using LiteTorrent.Domain.Services.LocalStorage.Configuration;
using Microsoft.Extensions.Configuration;

namespace LiteTorrent.Domain.Services.Common;

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
}