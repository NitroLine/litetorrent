using LiteTorrent.Sdk.Sharding;

namespace LiteTorrent.Sdk;

public class DistributionContext
{
    public List<ShardedFile> DistributingFiles { get; }
}