namespace LiteTorrent.Domain.Services.LocalStorage.SharedFiles;

public record SharedFileBuildInfo(
    string Name, 
    ulong SizeInBytes, 
    uint ShardMaxSizeInBytes, 
    IReadOnlyList<Hash> ShardHashes
);