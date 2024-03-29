﻿using LiteTorrent.Core;
using LiteTorrent.Domain.Services.Common.Serialization;
using LiteTorrent.Domain.Services.LocalStorage.SharedFiles;
using MessagePack;
using MessagePipe;

namespace LiteTorrent.Domain.Services.Commands;

public record AddSharedFileCommand(
    string AbsoluteFilePath
);

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
        await using var torrentFile = new FileStream(
            request.AbsoluteFilePath, 
            FileMode.Open, 
            FileAccess.Read);
                    
        var dto = MessagePackSerializer.Deserialize<DtoMessagePackTorrentFile>(
            torrentFile, 
            SerializerHelper.DefaultOptions,
            cancellationToken);
        
        var createInfo = new SharedFileCreateInfo(
            dto.RelativePath,
            dto.ShardMaxSizeInBytes);
        
        var createResult = await sharedFileRepository.Save(
            dto.Hash,
            (long)dto.SizeInBytes,
            createInfo,
            cancellationToken);
        
        return createResult.TryGetError(out _, out var error) ? error : Result.Ok;
    }
}