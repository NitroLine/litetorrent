namespace LiteTorrent.Domain.Services.PieceExchange.Messages;

public abstract class MessageHandler<TMessage> : IMessageHandler<TMessage>
{
    public abstract Task<HandleResult> Handle(
        ConnectionContext context,
        TMessage message,
        CancellationToken cancellationToken);

    public Task<HandleResult> Handle(ConnectionContext context, object payload, CancellationToken cancellationToken)
    {
        if (payload is not TMessage typedPayload)
            throw new InvalidCastException($"Expected type {typeof(TMessage)}, but was {payload.GetType()}");

        return Handle(context, typedPayload, cancellationToken);
    }
}