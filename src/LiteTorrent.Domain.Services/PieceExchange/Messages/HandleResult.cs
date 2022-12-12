namespace LiteTorrent.Domain.Services.PieceExchange.Messages;

public record HandleResult(bool IsNeedToSend, object Payload)
{
    public static readonly HandleResult OkNotSend = new(false, new object());
}