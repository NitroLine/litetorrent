using System.Net;

namespace LiteTorrent.Domain.Services.LocalStorage.SharedFiles;

public record SharedFileCreateInfo(
    IReadOnlyList<DnsEndPoint> Trackers,
    string RelativePath, 
    ulong SizeInBytes, 
    uint ShardMaxSizeInBytes
);