using LiteTorrent.Core;
using LiteTorrent.Domain.Services.LocalStorage.SharedFiles;
using MessagePipe;

namespace LiteTorrent.Domain.Services.Commands;

public record GetSharedFilesCommand;

public class GetSharedFilesCommandHandler 
    : IAsyncRequestHandler<GetSharedFilesCommand, Result<SharedFile[]>>
{
    private readonly SharedFileRepository sharedFileRepository;

    public GetSharedFilesCommandHandler(SharedFileRepository sharedFileRepository)
    {
        this.sharedFileRepository = sharedFileRepository;
    }
    
    public async ValueTask<Result<SharedFile[]>> InvokeAsync(
        GetSharedFilesCommand request,
        CancellationToken cancellationToken)
    {
        var getAllResult = await sharedFileRepository.GetAll(cancellationToken);
        
        return getAllResult.TryGetError(out var sharedFiles, out var error) 
            ? error
            : sharedFiles.ToArray();
    }
}