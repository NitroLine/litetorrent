namespace LiteTorrent.Domain.Services.ShardExchange.Messages;

public record HandleResult(bool IsNeedToSend, object? Payload)
{
    public static readonly HandleResult OkNotSend = new HandleResult(false, null);
}