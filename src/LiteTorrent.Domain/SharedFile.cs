using System.Net;
using LiteTorrent.Infra;

namespace LiteTorrent.Domain;

public class SharedFile
{
    public SharedFile(
        Hash id,
        IReadOnlyList<DnsEndPoint> trackers, 
        string relativePath, 
        ulong sizeInBytes,
        uint shardMaxSizeInBytes, 
        IReadOnlyList<Hash> shardHashes)
    {
        Id = id;
        Trackers = trackers;
        RelativePath = relativePath;
        SizeInBytes = sizeInBytes;
        ShardMaxSizeInBytes = shardMaxSizeInBytes;
        ShardHashes = shardHashes;
    }
    
    public Hash Id { get; } // TODO: may be in inconsistent state 
    public IReadOnlyList<DnsEndPoint> Trackers { get; }
    public string RelativePath { get; }
    public ulong SizeInBytes { get; } // TODO: may be in inconsistent state 
    public uint ShardMaxSizeInBytes { get; }
    public IReadOnlyList<Hash> ShardHashes { get; } // TODO: 100gb file = 6.25gb hash concat. Too many...
}