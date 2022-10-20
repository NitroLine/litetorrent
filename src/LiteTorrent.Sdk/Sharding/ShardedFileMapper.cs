using LiteTorrent.Sdk.TorrentFileEntities;

namespace LiteTorrent.Sdk.Sharding;

public static class ShardedFileMapper
{
    public static ShardedFile MapFrom(DistributingFileInfo fileInfo)
    {
        var shards = fileInfo.ShardHashes
            .Select((hash, index) => new Shard(index * fileInfo.ShardSizeInBytes, hash))
            .ToList();
        
        return new ShardedFile(
            fileInfo.FullName,
            fileInfo.SizeInBytes,
            fileInfo.ShardSizeInBytes,
            shards,
            ShardingHelper.CalculateMerkelTreeRootHash(shards));
    }
}