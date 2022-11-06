namespace LiteTorrent.Domain.Services.LocalStorage.Shards;

public static class ShardHelper
{
    public static byte[] CreateShardBuffer(SharedFile sharedFile, ulong index)
    {
        var bufferSize = index == sharedFile.GetLastShardIndex()
            ? sharedFile.SizeInBytes - index * sharedFile.ShardMaxSizeInBytes
            : sharedFile.ShardMaxSizeInBytes;
        
        return new byte[bufferSize];
    }
}