using LiteTorrent.Core;
using LiteTorrent.Domain.Services.Common.Serialization;
using LiteTorrent.Domain.Services.LocalStorage.SharedFiles;
using MessagePack;
using MessagePipe;

namespace LiteTorrent.Domain.Services.Commands;

/// <summary>
/// Command to create torrent-file and register it in the client.
/// </summary>
public record CreateSharedFileCommand(
    string RelativePath,
    uint ShardMaxSizeInBytes,
    string OutputFileAbsolutePath
);

public class CreateSharedFileCommandHandler
    : IAsyncRequestHandler<CreateSharedFileCommand, Result<Unit>>
{
    private readonly SharedFileRepository sharedFileRepository;

    public CreateSharedFileCommandHandler(SharedFileRepository sharedFileRepository)
    {
        this.sharedFileRepository = sharedFileRepository;
    }
    
    public async ValueTask<Result<Unit>> InvokeAsync(
        CreateSharedFileCommand request, 
        CancellationToken cancellationToken)
    {
        var createInfo = new SharedFileCreateInfo(request.RelativePath, request.ShardMaxSizeInBytes);
        var createResult = await sharedFileRepository.Create(createInfo, cancellationToken);
        if (createResult.TryGetError(out var hash, out var error))
            return error;

        var getResult = await sharedFileRepository.Get(hash, cancellationToken);
        if (getResult.TryGetError(out var sharedFile, out error))
            return error;

        var torrentFile = new DtoMessagePackTorrentFile(
            sharedFile.Hash,
            sharedFile.RelativePath,
            sharedFile.SizeInBytes,
            sharedFile.PieceMaxSizeInBytes);
        
        await using var outputFile = new FileStream(
            request.OutputFileAbsolutePath, 
            FileMode.OpenOrCreate, 
            FileAccess.Write);

        await MessagePackSerializer.SerializeAsync(
            outputFile, 
            torrentFile,
            SerializerHelper.DefaultOptions,
            cancellationToken);

        return Result.Ok;
    }
}