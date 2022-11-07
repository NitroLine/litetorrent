namespace LiteTorrent.Domain;

public static class SharedFileExtensions
{
    public static ulong GetLastShardIndex(this SharedFile sharedFile)
    {
        return sharedFile.SizeInBytes / sharedFile.ShardMaxSizeInBytes 
               + (sharedFile.SizeInBytes % sharedFile.ShardMaxSizeInBytes == 0 ? 0u : 1u);
    }

    public static ulong GetShardOffsetByIndex(this SharedFile sharedFile, ulong index)
    {
        return sharedFile.ShardMaxSizeInBytes * index;
    }
}