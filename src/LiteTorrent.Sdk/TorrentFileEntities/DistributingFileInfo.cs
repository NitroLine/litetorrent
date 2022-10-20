using LiteTorrent.Sdk.Misc;

namespace LiteTorrent.Sdk.TorrentFileEntities;

public record DistributingFileInfo(
    string FullName, 
    long SizeInBytes, 
    int ShardSizeInBytes,
    IReadOnlyList<Hash> ShardHashes
);