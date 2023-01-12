namespace LiteTorrent.Core;

public static class TaskExtensions
{
    public static async Task<TResult> WithTimeout<TResult>(this Task<TResult> task, int milliseconds = 10000)
    {
        if (await Task.WhenAny(task, Task.Delay(milliseconds)) == task)
            return task.Result;

        throw new TimeoutException();
    }
    
    public static async Task WithTimeout(this Task task, int milliseconds = 10000)
    {
        if (await Task.WhenAny(task, Task.Delay(milliseconds)) == task)
            return;

        throw new TimeoutException();
    }

    public static async IAsyncEnumerable<TValue> WithTimeoutForMoveNext<TValue>(
        this IAsyncEnumerable<TValue> asyncEnumerable,
        int milliseconds = 10000)
    {
        var enumerator = asyncEnumerable.GetAsyncEnumerator();
        while (await enumerator.MoveNextAsync().AsTask().WithTimeout(milliseconds))
        {
            yield return enumerator.Current;
        }
    }
}