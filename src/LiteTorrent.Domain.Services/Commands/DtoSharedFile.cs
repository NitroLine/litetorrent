using System.Net;
using MessagePack;

namespace LiteTorrent.Domain.Services.InterProcessProtocol.Server.Serialization;

[MessagePackObject]
public record DtoSharedFile(
    [property: Key(0)] Hash RootHash,
    [property: Key(1)] IReadOnlyList<DnsEndPoint> Trackers,
    [property: Key(2)] string RelativePath, 
    [property: Key(3)] ulong SizeInBytes,
    [property: Key(4)] uint ShardMaxSizeInBytes
);