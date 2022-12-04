using System.Runtime.CompilerServices;
using LiteTorrent.Domain.Services.Common;
using LiteTorrent.Domain.Services.LocalStorage.Common;
using LiteTorrent.Infra;

namespace LiteTorrent.Domain.Services.LocalStorage.Shards;

public class ShardReader
{
    private readonly SharedFile sharedFile;
    private readonly string basePath;
    private readonly Dictionary<Hash, ulong> shardIndexByHash;

    private ShardReader(
        SharedFile sharedFile,
        string basePath,
        Dictionary<Hash, ulong> shardIndexByHash)
    {
        this.sharedFile = sharedFile;
        this.basePath = basePath;
        this.shardIndexByHash = shardIndexByHash;
    }

    public static Result<ShardReader> Create(SharedFile sharedFile, string basePath)
    {
        var path = Path.Join(basePath, sharedFile.RelativePath);
        if (!File.Exists(path))
            throw new FileNotFoundException();

        var shardIndexByHash = new Dictionary<Hash, ulong>();
        for (var i = 0u; i < sharedFile.ShardHashes.Count; i++)
            shardIndexByHash[sharedFile.ShardHashes[(int)i]] = i; // TODO: delete casts uint to int?
        return new ShardReader(
            sharedFile, 
            basePath, 
            shardIndexByHash);
    }

    // ReSharper disable once ParameterTypeCanBeEnumerable.Global
    public async Task<IReadOnlyList<Result<Shard>>> Read(IReadOnlyList<Hash> shardHashes, CancellationToken cancellationToken)
    {
        await using var fileStream = LocalStorageHelper.GetFileStreamToRead(basePath);

        var shards = new List<Result<Shard>>();
        foreach (var hash in shardHashes)
        {
            var readResult = await ReadFromStream(fileStream, hash, cancellationToken);
            if (readResult.TryGetError(out var shard, out var error))
            {
                shards.Add(error);
                continue;
            }
            
            shards.Add(shard);
        }

        return shards;
    }
    
    public async IAsyncEnumerable<Hash> GetShardsToComplete(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var availableShardHashes = await GetAvailableShardHashes(cancellationToken)
            .ToHashSetAsync(cancellationToken);

        foreach (var shardHash in sharedFile.ShardHashes)
        {
            if (availableShardHashes.Contains(shardHash))
                continue;

            yield return shardHash;
        }
    }
    
    private async IAsyncEnumerable<Hash> GetAvailableShardHashes( // ISharedFileRepo -> SharedFile -> shardSize -> splitting: available is only not empty shards
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await using var fileStream = LocalStorageHelper.GetFileStreamToRead(basePath);
        
        var emptyHash = Hash.CreateFromRaw(new byte[sharedFile.ShardMaxSizeInBytes]);
        await foreach (var shardData in ReadSequentially(cancellationToken))
        {
            var hash = Hash.CreateFromRaw(shardData);
            if (hash != emptyHash)
                yield return hash;
        }
    }

    private async IAsyncEnumerable<ReadOnlyMemory<byte>> ReadSequentially(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await using var fileStream = LocalStorageHelper.GetFileStreamToRead(basePath);
        
        var buffer = new byte[sharedFile.ShardMaxSizeInBytes];
        var count = await fileStream.ReadAsync(buffer, cancellationToken);
        while (count != 0)
        {
            yield return new ReadOnlyMemory<byte>(buffer, 0, count);
            count = await fileStream.ReadAsync(buffer, cancellationToken);
        }
    }

    private async Task<Result<Shard>> ReadFromStream(
        Stream stream,
        Hash shardHash,
        CancellationToken cancellationToken)
    {
        if (!shardIndexByHash.TryGetValue(shardHash, out var index))
            return ErrorRegistry.ShardNotFound(shardHash);

        var buffer = ShardHelper.CreateShardBuffer(sharedFile, index);
        stream.Seek((long)sharedFile.GetShardOffsetByIndex(index), SeekOrigin.Begin);
        var count = await stream.ReadAsync(buffer, cancellationToken);
        if (count != buffer.Length)
        {
            throw new InvalidOperationException(
                $"Shard size is incorrect. Expected: '{buffer.Length}'. Was: '{count}'");
        }

        return new Shard(shardHash, buffer);
    }
}