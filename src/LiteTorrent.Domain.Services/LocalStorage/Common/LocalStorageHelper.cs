namespace LiteTorrent.Domain.Services.LocalStorage.Common;

public static class LocalStorageHelper
{
    private static readonly FileStreamOptions DefaultFileStreamOptionsToRead = new(); 
    
    private static readonly FileStreamOptions DefaultFileStreamOptionsToWrite = new()
    {
        Access = FileAccess.Write, 
        Share = FileShare.None
    }; 
    
    public static FileStream GetFileStreamToWrite(string path)
    {
        return new FileStream(path, DefaultFileStreamOptionsToWrite);
    }
    
    public static FileStream GetFileStreamToRead(string path)
    {
        return new FileStream(path, DefaultFileStreamOptionsToRead);
    }
}