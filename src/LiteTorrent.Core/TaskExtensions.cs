namespace LiteTorrent.Core;

public static class TaskExtensions
{
    public static async Task<TResult> WithTimeout<TResult>(this Task<TResult> task, int milliseconds = 30000)
    {
        if (await Task.WhenAny(task, Task.Delay(milliseconds)) == task)
            return task.Result;

        throw new TimeoutException();
    }
    
    public static async Task WithTimeout(this Task task, int milliseconds = 30000)
    {
        if (await Task.WhenAny(task, Task.Delay(milliseconds)) == task)
            return;

        throw new TimeoutException();
    }
}