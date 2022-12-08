using System.Net;
using MessagePack;

namespace LiteTorrent.Domain.Services.Commands;

[MessagePackObject]
public record DtoUserSharedFile(
    [property: Key("Hash")] Hash Hash,
    [property: Key("Trackers")] IReadOnlyList<DnsEndPoint> Trackers,
    [property: Key("RelativePath")] string RelativePath, 
    [property: Key("SizeInBytes")] ulong SizeInBytes,
    [property: Key("ShardMaxSizeInBytes")] uint ShardMaxSizeInBytes
);