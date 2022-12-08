using LiteTorrent.Domain.Services.LocalStorage.HashTrees;
using LiteTorrent.Domain.Services.LocalStorage.Shards;
using MessagePack;

namespace LiteTorrent.Domain.Services.ShardExchange.Messages;

[MessagePackObject]
public record ShardRequestMessage(
    [property: Key(0)] ulong Index
);

public class ShardRequestMessageHandler : MessageHandler<ShardRequestMessage>
{
    private readonly ShardRepository shardRepository;
    private readonly HashTreeRepository hashTreeRepository;

    public ShardRequestMessageHandler(ShardRepository shardRepository, HashTreeRepository hashTreeRepository)
    {
        this.shardRepository = shardRepository;
        this.hashTreeRepository = hashTreeRepository;
    }
    
    public override async Task<HandleResult> Handle(
        ConnectionContext context, 
        ShardRequestMessage message,
        CancellationToken cancellationToken)
    {
        var reader = await shardRepository.CreateReader(context.SharedFile.Hash, cancellationToken);
        var readResult = await reader.Read(message.Index, cancellationToken);
        if (readResult.TryGetError(out var shard, out var error))
            throw new InvalidOperationException(error.Message);

        var confirmationPath = context.SharedFile.HashTree.GetPath((int)message.Index).ToArray();

        // TODO: reduce data copying
        return new HandleResult(true, new ShardResponseMessage(shard.Index, shard.Data.ToArray(), confirmationPath));
    }
}