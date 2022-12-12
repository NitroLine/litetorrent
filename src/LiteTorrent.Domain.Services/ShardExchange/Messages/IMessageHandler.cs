namespace LiteTorrent.Domain.Services.ShardExchange.Messages;

public interface IMessageHandler
{
    Task<HandleResult> Handle(ConnectionContext context, object payload, CancellationToken cancellationToken);
}