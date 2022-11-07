namespace LiteTorrent.Domain;

public record Shard(Hash Hash, ReadOnlyMemory<byte> Data);