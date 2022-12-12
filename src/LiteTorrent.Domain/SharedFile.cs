using System.Net;

namespace LiteTorrent.Domain;

/// <summary>
/// Meta information about file distributing in torrent network
/// </summary>
public class SharedFile
{
    public SharedFile(
        MerkleTree hashTree,
        IReadOnlyList<DnsEndPoint> trackers, 
        string relativePath, 
        ulong sizeInBytes,
        uint pieceMaxSizeInBytes)
    {
        HashTree = hashTree;
        Trackers = trackers;
        RelativePath = relativePath;
        SizeInBytes = sizeInBytes;
        PieceMaxSizeInBytes = pieceMaxSizeInBytes;
    }
    
    public MerkleTree HashTree { get; }
    public Hash Hash => HashTree.RootHash;
    public IReadOnlyList<DnsEndPoint> Trackers { get; }
    public string RelativePath { get; }
    public ulong SizeInBytes { get; }
    public uint PieceMaxSizeInBytes { get; }
    public ulong ShardCount => SizeInBytes / PieceMaxSizeInBytes;
}