using LiteTorrent.Sdk.Sharding;

namespace LiteTorrent.Sdk.DataExchange;

public class Distribution
{
    public List<ShardedFile> DistributingFiles { get; }
}