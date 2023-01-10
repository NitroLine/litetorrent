namespace LiteTorrent.Core;

public class FilePool
{
    private readonly Dictionary<string, SemaphoreSlim> semaphoreByKey = new();
    private readonly SemaphoreSlim dictSemaphore = new(1, 1);
    
    public async Task<FileLock> GetAsync(string fullFileName, FileStreamOptions options)
    {
        var semaphoresToWait = await GetSemaphores(fullFileName);

        foreach (var semaphore in semaphoresToWait) 
            await semaphore.WaitAsync();
        
        try
        {
            return new FileLock(new FileStream(fullFileName, options), semaphoresToWait[0]);
        }
        catch
        {
            semaphoresToWait[0].Release();
            throw;
        }
    }

    private async Task<SemaphoreSlim[]> GetSemaphores(params string[] keys)
    {
        var semaphoresToWait = new SemaphoreSlim[keys.Length];
        
        var isLockAcquired = false;
        try
        {
            await dictSemaphore.WaitAsync();
            isLockAcquired = true;

            for (var i = 0; i < keys.Length; i++)
            {
                var key = keys[i];

                if (!semaphoreByKey.TryGetValue(key, out var semaphoreSlim))
                {
                    semaphoreSlim = new SemaphoreSlim(1, 1);
                    semaphoreByKey[key] = semaphoreSlim;
                }
                
                semaphoresToWait[i] = semaphoreSlim;
            }
        }
        finally
        {
            if (isLockAcquired)
                dictSemaphore.Release();
        }

        return semaphoresToWait;
    }
}