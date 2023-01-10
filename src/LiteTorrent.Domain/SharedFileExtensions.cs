namespace LiteTorrent.Domain;

public static class SharedFileExtensions
{
    public static ulong GetLastShardIndex(this SharedFile sharedFile)
    {
        return sharedFile.SizeInBytes / sharedFile.PieceMaxSizeInBytes 
               + (sharedFile.SizeInBytes % sharedFile.PieceMaxSizeInBytes == 0 ? 0u : 1u) - 1;
    }

    public static ulong GetShardOffsetByIndex(this SharedFile sharedFile, ulong index)
    {
        return sharedFile.PieceMaxSizeInBytes * index;
    }
}