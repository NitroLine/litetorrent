using LiteTorrent.Domain.Services.LocalStorage.HashTrees;
using LiteTorrent.Domain.Services.LocalStorage.Pieces;
using LiteTorrent.Domain.Services.ShardExchange.Messages;
using MessagePack;

namespace LiteTorrent.Domain.Services.PieceExchange.Messages;

[MessagePackObject]
public record PieceRequestMessage(
    [property: Key(0)] ulong Index
);

public class PieceRequestMessageHandler : MessageHandler<PieceRequestMessage>
{
    private readonly PieceRepository pieceRepository;
    private readonly HashTreeRepository hashTreeRepository;

    public PieceRequestMessageHandler(PieceRepository pieceRepository, HashTreeRepository hashTreeRepository)
    {
        this.pieceRepository = pieceRepository;
        this.hashTreeRepository = hashTreeRepository;
    }
    
    public override async Task<HandleResult> Handle(
        ConnectionContext context, 
        PieceRequestMessage message,
        CancellationToken cancellationToken)
    {
        var reader = await pieceRepository.CreateReader(context.SharedFile.Hash, cancellationToken);
        var readResult = await reader.Read(message.Index, cancellationToken);
        if (readResult.TryGetError(out var shard, out var error))
            throw new InvalidOperationException(error.Message);

        var confirmationPath = context.SharedFile.HashTree.GetPath((int)message.Index).ToArray();

        // TODO: reduce data copying
        return new HandleResult(true, new PieceResponseMessage(shard.Index, shard.Data.ToArray(), confirmationPath));
    }
}