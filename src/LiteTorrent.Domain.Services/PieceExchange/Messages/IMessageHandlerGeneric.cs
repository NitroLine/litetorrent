namespace LiteTorrent.Domain.Services.PieceExchange.Messages;

public interface IMessageHandler<in TMessage> : IMessageHandler
{
    Task<HandleResult> Handle(ConnectionContext context, TMessage message, CancellationToken cancellationToken);
}