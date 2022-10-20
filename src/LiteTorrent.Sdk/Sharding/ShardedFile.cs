using System.Runtime.CompilerServices;
using LiteTorrent.Sdk.Misc;

namespace LiteTorrent.Sdk.Sharding;

public record ShardedFile(
    string FullName,
    long SizeInBytes,
    int ShardSizeInBytes,
    IReadOnlyList<Shard> Shards,
    Hash Hash
)
{
    public async IAsyncEnumerable<Shard> GetMissingShards(
        FileInfo fileInfo, 
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var index = 0;
        var diskShards = ShardingHelper.GetShards(fileInfo, ShardSizeInBytes, cancellationToken);
        // ReSharper disable once UseCancellationTokenForIAsyncEnumerable
        await foreach (var diskShard in diskShards)
        {
            if (diskShard.Hash == Shards[index].Hash)
                continue;

            yield return diskShard;
            
            index++;
        }
    }
    
    public static async Task<ShardedFile> Create(
        FileInfo fileInfo, 
        int shardSizeInBytes, 
        CancellationToken cancellationToken)
    {
        if (!fileInfo.Exists)
            throw new ArgumentException($"File {fileInfo.FullName} is not exist");

        if (shardSizeInBytes < 0)
            throw new ArgumentException("Shard size must be greater than zero");

        var shards = await ShardingHelper
            .GetShards(fileInfo, shardSizeInBytes, cancellationToken)
            .ToListAsync(cancellationToken: cancellationToken)
            .AsTask();
        
        return new ShardedFile(
            fileInfo.FullName,
            fileInfo.Length,
            shardSizeInBytes,
            shards,
            ShardingHelper.CalculateMerkelTreeRootHash(shards));
    }
}