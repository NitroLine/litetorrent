﻿using LiteTorrent.Core;
using LiteTorrent.Domain.Services.LocalStorage.Common;

namespace LiteTorrent.Domain.Services.LocalStorage.Pieces;

public class PieceReader
{
    private readonly SharedFile sharedFile;
    private readonly string basePath;

    public PieceReader(SharedFile sharedFile, string basePath)
    {
        var path = Path.Join(basePath, sharedFile.RelativePath);
        if (!File.Exists(path))
            throw new FileNotFoundException();
        
        this.sharedFile = sharedFile;
        this.basePath = basePath;
    }

    // ReSharper disable once ParameterTypeCanBeEnumerable.Global
    public async Task<Result<Piece>> Read(
        ulong index, 
        CancellationToken cancellationToken)
    {
        await using var fileStream = LocalStorageHelper.GetFileStreamToRead(basePath);

        var readResult = await ReadFromStream(fileStream, index, cancellationToken);
        if (readResult.TryGetError(out var shard, out var error))
            return error;

        return shard;
    }

    private async Task<Result<Piece>> ReadFromStream(
        Stream stream,
        ulong index,
        CancellationToken cancellationToken)
    {
        var buffer = PieceHelper.CreateShardBuffer(sharedFile, index);
        stream.Seek((long)sharedFile.GetShardOffsetByIndex(index), SeekOrigin.Begin);
        var count = await stream.ReadAsync(buffer, cancellationToken);
        if (count != buffer.Length)
        {
            throw new InvalidOperationException(
                $"Shard size is incorrect. Expected: '{buffer.Length}'. Was: '{count}'");
        }

        return new Piece(index, buffer);
    }
}