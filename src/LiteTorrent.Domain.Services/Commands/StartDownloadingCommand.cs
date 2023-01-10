using System.Net;
using LiteTorrent.Core;
using LiteTorrent.Domain.Services.LocalStorage.SharedFiles;
using LiteTorrent.Domain.Services.PieceExchange;
using MessagePipe;

namespace LiteTorrent.Domain.Services.Commands;

public record StartDownloadingCommand(Hash Hash, IPEndPoint[] Hosts);

public class StartDownloadingCommandHandler
    : IAsyncRequestHandler<StartDownloadingCommand, Result<Unit>>
{
    private readonly SharedFileRepository sharedFileRepository;
    private readonly PieceExchanger pieceExchanger;

    public StartDownloadingCommandHandler(
        SharedFileRepository sharedFileRepository,
        PieceExchanger pieceExchanger)
    {
        this.sharedFileRepository = sharedFileRepository;
        this.pieceExchanger = pieceExchanger;
    }
    
    public async ValueTask<Result<Unit>> InvokeAsync(
        StartDownloadingCommand request,
        CancellationToken cancellationToken)
    {
        var getResult = await sharedFileRepository.Get(request.Hash, cancellationToken);
        if (getResult.TryGetError(out var sharedFile, out var error))
            return error;

        await pieceExchanger.StartDownloading(request.Hosts, sharedFile, cancellationToken);
        
        return Result.Ok;
    }
}