namespace LiteTorrent.Domain.Services.PieceExchange.Messages;

public interface IMessageHandler
{
    Task<HandleResult> Handle(ConnectionContext context, object payload, CancellationToken cancellationToken);
}