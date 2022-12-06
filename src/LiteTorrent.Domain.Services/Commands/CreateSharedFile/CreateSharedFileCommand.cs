using System.Net;
using LiteTorrent.Core;
using LiteTorrent.Domain.Services.Common.Serialization;
using LiteTorrent.Domain.Services.LocalStorage.SharedFiles;
using MessagePack;
using MessagePipe;

namespace LiteTorrent.Domain.Services.Commands.CreateSharedFile;

[MessagePackObject]
public record CreateSharedFileCommand(
    [property: Key(0)] IReadOnlyList<DnsEndPoint> Trackers,
    [property: Key(1)] string RelativePath,
    [property: Key(2)] ulong SizeInBytes,
    [property: Key(3)] uint ShardMaxSizeInBytes,
    [property: Key(4)] string OutputFileAbsolutePath
);

public class CreateSharedFileCommandHandler
    : IAsyncRequestHandler<CreateSharedFileCommand, Result<DtoSharedFile>>
{
    private readonly SharedFileRepository sharedFileRepository;

    public CreateSharedFileCommandHandler(SharedFileRepository sharedFileRepository)
    {
        this.sharedFileRepository = sharedFileRepository;
    }
    
    public async ValueTask<Result<DtoSharedFile>> InvokeAsync(
        CreateSharedFileCommand request, 
        CancellationToken cancellationToken)
    {
        var createInfo = new SharedFileCreateInfo(
            request.Trackers, 
            request.RelativePath,
            request.SizeInBytes,
            request.ShardMaxSizeInBytes);
        
        var createResult = await sharedFileRepository.Create(createInfo, cancellationToken);
        if (createResult.TryGetError(out var hash, out var error))
            return error;

        var torrentFile = new DtoSharedFile(
            hash,
            request.Trackers, 
            request.RelativePath,
            request.SizeInBytes,
            request.ShardMaxSizeInBytes);
        
        await using var outputFile = new FileStream(
            request.OutputFileAbsolutePath, 
            FileMode.OpenOrCreate, 
            FileAccess.Write);

        await MessagePackSerializer.SerializeAsync(
            outputFile, 
            torrentFile,
            SerializerHelper.SerializerOptions,
            cancellationToken);

        return torrentFile;
    }
}