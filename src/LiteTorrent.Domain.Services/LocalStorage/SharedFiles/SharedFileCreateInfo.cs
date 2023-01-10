namespace LiteTorrent.Domain.Services.LocalStorage.SharedFiles;

public record SharedFileCreateInfo(
    string RelativePath,
    uint ShardMaxSizeInBytes
);