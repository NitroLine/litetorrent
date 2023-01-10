using MessagePack;

namespace LiteTorrent.Domain.Services.Commands;

[MessagePackObject]
public record DtoMessagePackTorrentFile(
    [property: Key("Hash")] Hash Hash,
    [property: Key("RelativePath")] string RelativePath, 
    [property: Key("SizeInBytes")] ulong SizeInBytes,
    [property: Key("ShardMaxSizeInBytes")] uint ShardMaxSizeInBytes
);