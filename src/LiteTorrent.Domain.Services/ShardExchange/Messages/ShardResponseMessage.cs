using LiteTorrent.Domain.Services.LocalStorage.Shards;
using MessagePack;

namespace LiteTorrent.Domain.Services.ShardExchange.Messages;

[MessagePackObject]
public record ShardResponseMessage(
    [property: Key(0)] ulong Index,
    [property: Key(1)] byte[] Payload,
    [property: Key(2)] Hash[] ConfirmationPath
);

public class ShardResponseMessageHandler : MessageHandler<ShardResponseMessage>
{
    private readonly ShardRepository shardRepository;

    public ShardResponseMessageHandler(ShardRepository shardRepository)
    {
        this.shardRepository = shardRepository;
    }
    
    public override async Task<HandleResult> Handle(
        ConnectionContext context,
        ShardResponseMessage message,
        CancellationToken cancellationToken)
    {
        var writer = await shardRepository.CreateWriter(context.SharedFile.Hash, cancellationToken);
        var writeResult = await writer.Write(new Shard(message.Index, message.Payload), cancellationToken);
        if (writeResult.TryGetError(out _, out var error))
            throw new InvalidOperationException(error.Message);
        
        return HandleResult.OkNotSend;
    }
}