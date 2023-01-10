using MessagePack;

namespace LiteTorrent.Domain.Services.LocalStorage.Serialization;

[MessagePackObject]
public record DtoSharedFile(
    [property: Key(0)] string RelativePath,
    [property: Key(1)] ulong SizeInBytes,
    [property: Key(2)] uint ShardMaxSizeInBytes
);