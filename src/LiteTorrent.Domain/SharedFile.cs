using System.Net;

namespace LiteTorrent.Domain;

/// <summary>
/// Meta information about file distributing in torrent network
/// </summary>
public class SharedFile
{
    public SharedFile(
        MerkelTree hashTree,
        IReadOnlyList<DnsEndPoint> trackers, 
        string relativePath, 
        ulong sizeInBytes,
        uint shardMaxSizeInBytes)
    {
        HashTree = hashTree;
        Trackers = trackers;
        RelativePath = relativePath;
        SizeInBytes = sizeInBytes;
        ShardMaxSizeInBytes = shardMaxSizeInBytes;
    }
    
    public MerkelTree HashTree { get; }
    public Hash Hash => HashTree.RootHash;
    public IReadOnlyList<DnsEndPoint> Trackers { get; }
    public string RelativePath { get; }
    public ulong SizeInBytes { get; }
    public uint ShardMaxSizeInBytes { get; }
    public ulong ShardCount => SizeInBytes / ShardMaxSizeInBytes;
}