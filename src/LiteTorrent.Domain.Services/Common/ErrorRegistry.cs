using LiteTorrent.Infra;

namespace LiteTorrent.Domain.Services.Common;

public static class ErrorRegistry
{
    public static Error ShardNotFound(Hash shardHash) => new($"Shard is not found {shardHash}");

    public static Error ShardIsTooLong(SharedFile sharedFile, ReadOnlyMemory<byte> shardData)
    {
        return new Error($"Shard {Convert.ToHexString(shardData.Span)} is too long ({shardData.Length} bytes) " +
                         $"for file {sharedFile.RelativePath}");
    }
}