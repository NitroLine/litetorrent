namespace LiteTorrent.Domain;

/// <summary>
/// Meta information about file distributing in torrent network
/// </summary>
public class SharedFile
{
    public SharedFile(
        MerkleTree hashTree,
        string relativePath, 
        ulong sizeInBytes,
        uint pieceMaxSizeInBytes)
    {
        HashTree = hashTree;
        RelativePath = relativePath;
        SizeInBytes = sizeInBytes;
        PieceMaxSizeInBytes = pieceMaxSizeInBytes;
    }
    
    public MerkleTree HashTree { get; }
    public Hash Hash => HashTree.RootHash;
    public string RelativePath { get; }
    public ulong SizeInBytes { get; }
    public uint PieceMaxSizeInBytes { get; }
    public ulong ShardCount => SizeInBytes / PieceMaxSizeInBytes;
}