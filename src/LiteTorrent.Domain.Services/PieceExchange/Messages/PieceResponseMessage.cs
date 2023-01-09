using LiteTorrent.Domain.Services.LocalStorage.Pieces;
using MessagePack;
using Microsoft.Extensions.Logging;

namespace LiteTorrent.Domain.Services.PieceExchange.Messages;

[MessagePackObject]
public record PieceResponseMessage(
    [property: Key(0)] ulong Index,
    [property: Key(1)] byte[] Payload,
    [property: Key(2)] Hash[] ConfirmationPath
);

public class PieceResponseMessageHandler : MessageHandler<PieceResponseMessage>
{
    private readonly PieceRepository pieceRepository;
    private readonly ILogger<PieceResponseMessageHandler> logger;

    public PieceResponseMessageHandler(
        PieceRepository pieceRepository, 
        ILogger<PieceResponseMessageHandler> logger)
    {
        this.pieceRepository = pieceRepository;
        this.logger = logger;
    }
    
    public override async Task<HandleResult> Handle(
        ConnectionContext context,
        PieceResponseMessage message,
        CancellationToken cancellationToken)
    {
        logger.LogDebug($"Response : {message.Index}");
        var writer = await pieceRepository.CreateWriter(context.SharedFile.Hash, cancellationToken);
        var writeResult = await writer.Write(new Piece(message.Index, message.Payload), cancellationToken);
        if (writeResult.TryGetError(out _, out var error))
            throw new InvalidOperationException(error.Message);
        
        return HandleResult.OkNotSend;
    }
}