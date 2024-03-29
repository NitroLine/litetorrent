﻿using LiteTorrent.Core;

namespace LiteTorrent.Domain.Services.Common;

public static class ErrorRegistry
{
    public static class Shard
    {
        public static Error IndexOutOfRange(SharedFile file, ulong foundIndex)
        {
            return new Error($"File {file.RelativePath} has {file.ShardCount} shards, but index was {foundIndex}");
        }
        
        public static Error ShardIsTooLong(SharedFile sharedFile, ReadOnlyMemory<byte> shardData)
        {
            return new Error($"Shard {Convert.ToHexString(shardData.Span)} is too long ({shardData.Length} bytes) " +
                             $"for file {sharedFile.RelativePath}");
        }
    }

    public static class Peer
    {
        public static Error ConnectionIsClosed()
        {
            return new Error("Connection is closed");
        }

        public static Error RequestedFileIsDownloading()
        {
            return new Error("Requested file is downloading");
        }
    }
}