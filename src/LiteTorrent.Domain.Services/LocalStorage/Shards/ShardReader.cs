using LiteTorrent.Core;
using LiteTorrent.Domain.Services.LocalStorage.Common;

namespace LiteTorrent.Domain.Services.LocalStorage.Shards;

public class ShardReader
{
    private readonly SharedFile sharedFile;
    private readonly string basePath;

    public ShardReader(SharedFile sharedFile, string basePath)
    {
        var path = Path.Join(basePath, sharedFile.RelativePath);
        if (!File.Exists(path))
            throw new FileNotFoundException();
        
        this.sharedFile = sharedFile;
        this.basePath = basePath;
    }

    // ReSharper disable once ParameterTypeCanBeEnumerable.Global
    public async Task<Result<Shard>> Read(
        ulong index, 
        CancellationToken cancellationToken)
    {
        await using var fileStream = LocalStorageHelper.GetFileStreamToRead(basePath);

        var readResult = await ReadFromStream(fileStream, index, cancellationToken);
        if (readResult.TryGetError(out var shard, out var error))
            return error;

        return shard;
    }

    private async Task<Result<Shard>> ReadFromStream(
        Stream stream,
        ulong index,
        CancellationToken cancellationToken)
    {
        var buffer = ShardHelper.CreateShardBuffer(sharedFile, index);
        stream.Seek((long)sharedFile.GetShardOffsetByIndex(index), SeekOrigin.Begin);
        var count = await stream.ReadAsync(buffer, cancellationToken);
        if (count != buffer.Length)
        {
            throw new InvalidOperationException(
                $"Shard size is incorrect. Expected: '{buffer.Length}'. Was: '{count}'");
        }

        return new Shard(index, buffer);
    }
}