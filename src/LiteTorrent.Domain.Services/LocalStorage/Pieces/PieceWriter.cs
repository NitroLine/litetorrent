using LiteTorrent.Core;
using LiteTorrent.Domain.Services.Common;
using LiteTorrent.Domain.Services.LocalStorage.Common;

namespace LiteTorrent.Domain.Services.LocalStorage.Pieces;

public class PieceWriter
{
    private readonly string fileFullName;
    private readonly SharedFile sharedFile;
    private readonly string basePath;

    public PieceWriter(
        SharedFile sharedFile,
        string basePath)
    {
        fileFullName = Path.Join(basePath, sharedFile.RelativePath);
        if (!File.Exists(fileFullName))
            throw new FileNotFoundException(fileFullName);
        
        this.sharedFile = sharedFile;
        this.basePath = basePath;
    }

    public async Task<Result<Unit>> Write(
        // ReSharper disable once ParameterTypeCanBeEnumerable.Global
        Piece piece,
        CancellationToken cancellationToken)
    {
        await using var fileLock = await LocalStorageHelper.FilePool.GetToWrite(fileFullName);
        
        var result = await WriteInStream(fileLock.FileStream, piece, cancellationToken);
        
        return result.TryGetError(out _, out var error) ? error : Result.Ok;
    }

    private async Task<Result<Unit>> WriteInStream(
        Stream stream, 
        Piece piece, 
        CancellationToken cancellationToken)
    {
        var (index, data) = piece;
        
        if (index >= sharedFile.ShardCount)
            return ErrorRegistry.Shard.IndexOutOfRange(sharedFile, index);

        if (data.Length > sharedFile.PieceMaxSizeInBytes)
            return ErrorRegistry.Shard.ShardIsTooLong(sharedFile, data);

        stream.Seek((long)sharedFile.GetShardOffsetByIndex(index), SeekOrigin.Begin);
        await stream.WriteAsync(data, cancellationToken);

        return Result.Ok;
    }
}