using System.Net;
using MessagePack;

namespace LiteTorrent.Domain.Services.Serialization;

[MessagePackObject]
public record DtoSharedFile(
    [property: Key(0)] IReadOnlyList<DnsEndPoint> Trackers,
    [property: Key(1)] string RelativePath,
    [property: Key(2)] ulong SizeInBytes,
    [property: Key(3)] uint ShardMaxSizeInBytes
);