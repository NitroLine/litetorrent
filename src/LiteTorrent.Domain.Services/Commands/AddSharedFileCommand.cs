using LiteTorrent.Core;
using LiteTorrent.Domain.Services.LocalStorage.SharedFiles;
using MessagePipe;

namespace LiteTorrent.Domain.Services.Commands;

public record AddSharedFileCommand(Hash FileHash, SharedFileCreateInfo CreateInfo);

public class AddSharedFileCommandHandler
    : IAsyncRequestHandler<AddSharedFileCommand, Result<Unit>>
{
    private readonly SharedFileRepository sharedFileRepository;

    public AddSharedFileCommandHandler(SharedFileRepository sharedFileRepository)
    {
        this.sharedFileRepository = sharedFileRepository;
    }
    
    public async ValueTask<Result<Unit>> InvokeAsync(
        AddSharedFileCommand request,
        CancellationToken cancellationToken)
    {
        var createResult = await sharedFileRepository.Create(request.FileHash, request.CreateInfo, cancellationToken);
        
        return createResult.TryGetError(out _, out var error) ? error : Result.Ok;
    }
}