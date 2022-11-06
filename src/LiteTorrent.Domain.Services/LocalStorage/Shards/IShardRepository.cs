namespace LiteTorrent.Domain.Services.LocalStorage.Shards;

public interface IShardRepository
{
    Task<ShardWriter> CreateWriter(Hash fileHash, CancellationToken cancellationToken);
    Task<ShardReader> CreateReader(Hash fileHash, CancellationToken cancellationToken);
}