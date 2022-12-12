using LiteTorrent.Domain.Services.ShardExchange.Messages;

namespace LiteTorrent.Domain.Services.PieceExchange.Messages;

public class HandlerResolver
{
    private static readonly HashSet<(Type, Type)> HandlerTypeByMessageType = new()
    {
        (typeof(PieceRequestMessage), typeof(PieceRequestMessageHandler)),
        (typeof(PieceResponseMessage), typeof(PieceResponseMessage))
    };

    private static Dictionary<Type, IMessageHandler>? handlerByMessageType;

    public HandlerResolver(IServiceProvider serviceProvider)
    {
        if (handlerByMessageType is not null)
            return;
        
        handlerByMessageType = new Dictionary<Type, IMessageHandler>();
        foreach (var (messageType, handlerType) in HandlerTypeByMessageType)
        {
            var handler = (IMessageHandler)serviceProvider.GetService(handlerType)! 
                          ?? throw new InvalidOperationException();
                
            handlerByMessageType[messageType] = handler;
        }
    }

#pragma warning disable CA1822
    // ReSharper disable once MemberCanBeMadeStatic.Global
    public IMessageHandler Resolve(object payload)
#pragma warning restore CA1822
    {
        return handlerByMessageType![payload.GetType()];
    }
}