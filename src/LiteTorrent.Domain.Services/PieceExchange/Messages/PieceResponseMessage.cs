using LiteTorrent.Domain.Services.LocalStorage.Pieces;
using LiteTorrent.Domain.Services.PieceExchange.Messages;
using MessagePack;

namespace LiteTorrent.Domain.Services.ShardExchange.Messages;

[MessagePackObject]
public record PieceResponseMessage(
    [property: Key(0)] ulong Index,
    [property: Key(1)] byte[] Payload,
    [property: Key(2)] Hash[] ConfirmationPath
);

public class PieceResponseMessageHandler : MessageHandler<PieceResponseMessage>
{
    private readonly PieceRepository pieceRepository;

    public PieceResponseMessageHandler(PieceRepository pieceRepository)
    {
        this.pieceRepository = pieceRepository;
    }
    
    public override async Task<HandleResult> Handle(
        ConnectionContext context,
        PieceResponseMessage message,
        CancellationToken cancellationToken)
    {
        var writer = await pieceRepository.CreateWriter(context.SharedFile.Hash, cancellationToken);
        var writeResult = await writer.Write(new Piece(message.Index, message.Payload), cancellationToken);
        if (writeResult.TryGetError(out _, out var error))
            throw new InvalidOperationException(error.Message);
        
        return HandleResult.OkNotSend;
    }
}