using LiteTorrent.Domain.Services.Common;
using LiteTorrent.Domain.Services.LocalStorage.Common;
using LiteTorrent.Infra;

namespace LiteTorrent.Domain.Services.LocalStorage.Shards;

public class ShardWriter
{
    private readonly SharedFile sharedFile;
    private readonly string basePath;

    public ShardWriter(
        SharedFile sharedFile,
        string basePath)
    {
        var path = Path.Join(basePath, sharedFile.RelativePath);
        if (!File.Exists(path))
            throw new FileNotFoundException();
        
        this.sharedFile = sharedFile;
        this.basePath = basePath;
    }

    public async Task<Result<Unit>> Write(
        // ReSharper disable once ParameterTypeCanBeEnumerable.Global
        Shard shard,
        CancellationToken cancellationToken)
    {
        await using var fileStream = LocalStorageHelper.GetFileStreamToWrite(basePath);
        
        var result = await WriteInStream(fileStream, shard, cancellationToken);
        
        return result.TryGetError(out _, out var error) ? error : Result.Ok;
    }

    private async Task<Result<Unit>> WriteInStream(
        Stream stream, 
        Shard shard, 
        CancellationToken cancellationToken)
    {
        var (index, data) = shard;
        
        if (index >= sharedFile.ShardCount)
            return ErrorRegistry.Shard.IndexOutOfRange(sharedFile, index);

        if (data.Length > sharedFile.ShardMaxSizeInBytes)
            return ErrorRegistry.Shard.ShardIsTooLong(sharedFile, data);

        stream.Seek((long)sharedFile.GetShardOffsetByIndex(index), SeekOrigin.Begin);
        await stream.WriteAsync(data, cancellationToken);

        return Result.Ok;
    }
}