namespace LiteTorrent.Core;

public sealed class FileLock : IAsyncDisposable
{
    private readonly SemaphoreSlim semaphore;

    public FileLock(FileStream fileStream, SemaphoreSlim semaphore)
    {
        FileStream = fileStream;
        this.semaphore = semaphore;
    }
    
    public FileStream FileStream { get; }

    public async ValueTask DisposeAsync()
    {
        await FileStream.DisposeAsync();
        semaphore.Release();
    }
}
