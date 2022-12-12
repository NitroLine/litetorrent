using Microsoft.Extensions.Logging;

namespace LiteTorrent.Domain.Services.Common;

public static class ExceptionHelper
{
    public static async Task HandleException<TContext>(Task task, ILogger<TContext> logger)
    {
        try
        {
            await task;
        }
        catch (Exception e)
        {
            logger.LogError(e, "error");
        }
    }
}