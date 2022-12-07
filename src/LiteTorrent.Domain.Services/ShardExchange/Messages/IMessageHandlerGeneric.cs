namespace LiteTorrent.Domain.Services.ShardExchange.Messages;

public interface IMessageHandler<in TMessage> : IMessageHandler
{
    Task<HandleResult> Handle(ConnectionContext context, TMessage message, CancellationToken cancellationToken);
}