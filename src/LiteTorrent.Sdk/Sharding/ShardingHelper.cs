using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace LiteTorrent.Sdk.Sharding;

public static class ShardingHelper
{
    public static async IAsyncEnumerable<Shard> GetShards(
        FileInfo fileInfo, 
        int shardSizeInBytes, 
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await using var fileStream = fileInfo.OpenRead();
        using var sha256Algorithm = SHA256.Create();

        var readByteCount = 0L;
        var buffer = new byte[shardSizeInBytes];
        while (readByteCount < fileInfo.Length)
        {
            var readByteCountOnIteration = await fileStream.ReadAsync(buffer, cancellationToken);

            var hash = sha256Algorithm.ComputeHash(buffer, 0, readByteCountOnIteration);
            yield return new Shard(readByteCount, hash);
            
            readByteCount += readByteCountOnIteration;
        }
    }
    
    public static byte[] CalculateMerkelTreeRootHash(IEnumerable<Shard> shards)
    {
        // TODO: to full binary tree?
        using var sha256Algorithm = SHA256.Create();
        return sha256Algorithm.ComputeHash(shards.SelectMany(shard => shard.Hash.Value).ToArray());
    }
}