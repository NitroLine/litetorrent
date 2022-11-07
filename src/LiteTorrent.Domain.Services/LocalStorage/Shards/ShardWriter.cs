using LiteTorrent.Domain.Services.Common;
using LiteTorrent.Domain.Services.LocalStorage.Common;
using LiteTorrent.Infra;

namespace LiteTorrent.Domain.Services.LocalStorage.Shards;

public class ShardWriter
{
    private readonly SharedFile sharedFile;
    private readonly string basePath;
    private readonly Dictionary<Hash, List<ulong>> shardOffsetByHash;

    private ShardWriter(
        SharedFile sharedFile,
        string basePath,
        Dictionary<Hash, List<ulong>> shardOffsetByHash)
    {
        this.sharedFile = sharedFile;
        this.basePath = basePath;
        this.shardOffsetByHash = shardOffsetByHash;
    }

    public static Result<ShardWriter> Create(SharedFile sharedFile, string basePath)
    {
        var path = Path.Join(basePath, sharedFile.RelativePath);
        if (!File.Exists(path))
            throw new FileNotFoundException();

        var shardOffsetByHash = new Dictionary<Hash, List<ulong>>();
        for (var i = 0u; i < sharedFile.ShardHashes.Count; i++)
        {
            if (!shardOffsetByHash.TryGetValue(sharedFile.ShardHashes[(int)i], out var offsets))
            {
                continue;
            }
            
            offsets.Add(i * sharedFile.ShardMaxSizeInBytes);
        }
        
        return new ShardWriter(
            sharedFile, 
            basePath,
            shardOffsetByHash);
    }

    public async Task<Result<Unit>> Write(
        // ReSharper disable once ParameterTypeCanBeEnumerable.Global
        IReadOnlyList<Shard> shards, 
        CancellationToken cancellationToken)
    {
        await using var fileStream = LocalStorageHelper.GetFileStreamToWrite(basePath);
        
        foreach (var shard in shards)
        {
            var result = await WriteInStream(fileStream, shard, cancellationToken);
            if (result.TryGetError(out _, out var error))
                return error;
        }
        
        return Result.Ok;
    }

    private async Task<Result<Unit>> WriteInStream(
        Stream stream, 
        Shard shard, 
        CancellationToken cancellationToken)
    {
        var (shardHash, data) = shard;
        
        if (!shardOffsetByHash.TryGetValue(shardHash, out var offsets))
            return ErrorRegistry.ShardNotFound(shardHash);

        if (data.Length > sharedFile.ShardMaxSizeInBytes)
            return ErrorRegistry.ShardIsTooLong(sharedFile, data);

        foreach (var offset in offsets)
        {
            stream.Seek((long)offset, SeekOrigin.Begin);
            await stream.WriteAsync(data, cancellationToken);
        }

        return Result.Ok;
    }
}