namespace LiteTorrent.Domain.Services.LocalStorage.Configuration;

public record LocalStorageConfiguration(
    string ShardDirectoryPath, 
    string HashTreeDirectoryPath,
    string SharedFileDirectoryPath
);