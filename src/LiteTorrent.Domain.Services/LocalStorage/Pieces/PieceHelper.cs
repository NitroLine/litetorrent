namespace LiteTorrent.Domain.Services.LocalStorage.Pieces;

public static class PieceHelper
{
    public static byte[] CreateShardBuffer(SharedFile sharedFile, ulong index)
    {
        var bufferSize = index == sharedFile.GetLastShardIndex()
            ? sharedFile.SizeInBytes - index * sharedFile.PieceMaxSizeInBytes
            : sharedFile.PieceMaxSizeInBytes;
        
        return new byte[bufferSize];
    }
}