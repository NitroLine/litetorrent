using LiteTorrent.Domain.Services.LocalStorage.Configuration;
using LiteTorrent.Domain.Services.LocalStorage.SharedFiles;

namespace LiteTorrent.Domain.Services.LocalStorage.Shards;

public class ShardRepository : IShardRepository
{
    private readonly ISharedFileRepository sharedFileRepository;
    private readonly FileSystemStorageConfiguration configuration;

    public ShardRepository(
        ISharedFileRepository sharedFileRepository, 
        FileSystemStorageConfiguration configuration)
    {
        this.sharedFileRepository = sharedFileRepository;
        this.configuration = configuration;
    }

    public async Task<ShardWriter> CreateWriter(Hash fileHash, CancellationToken cancellationToken)
    {
        var createResult = ShardWriter.Create(
            await sharedFileRepository.Get(fileHash, cancellationToken), 
            configuration.DirectoryPath);

        return createResult.TryGetError(out var reader, out var error) 
            ? throw new Exception(error.Message) 
            : reader; 
    }

    public async Task<ShardReader> CreateReader(Hash fileHash, CancellationToken cancellationToken)
    {
        var createResult = ShardReader.Create(
            await sharedFileRepository.Get(fileHash, cancellationToken), 
            configuration.DirectoryPath);

        return createResult.TryGetError(out var reader, out var error) 
            ? throw new Exception(error.Message) 
            : reader;
    }
}