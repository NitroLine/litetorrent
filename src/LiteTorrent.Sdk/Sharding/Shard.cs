using LiteTorrent.Sdk.Misc;

namespace LiteTorrent.Sdk.Sharding;

public record struct Shard(long Offset, Hash Hash);