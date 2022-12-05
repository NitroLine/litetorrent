namespace LiteTorrent.Domain;

public record struct Shard(ulong Index, ReadOnlyMemory<byte> Data);