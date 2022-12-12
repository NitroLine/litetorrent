using LiteTorrent.Domain.Services.LocalStorage.Configuration;
using LiteTorrent.Domain.Services.LocalStorage.SharedFiles;

namespace LiteTorrent.Domain.Services.LocalStorage.Shards;

public class ShardRepository
{
    private readonly SharedFileRepository sharedFileRepository;
    private readonly LocalStorageConfiguration configuration;

    public ShardRepository(
        SharedFileRepository sharedFileRepository, 
        LocalStorageConfiguration configuration)
    {
        this.sharedFileRepository = sharedFileRepository;
        this.configuration = configuration;
    }

    public async Task<ShardWriter> CreateWriter(Hash fileHash, CancellationToken cancellationToken)
    {
        var getResult = await sharedFileRepository.Get(fileHash, cancellationToken);
        if (getResult.TryGetError(out var sharedFile, out var error))
            throw new InvalidOperationException(error.Message);
        
        return new ShardWriter(sharedFile, configuration.ShardDirectoryPath);
    }

    public async Task<ShardReader> CreateReader(Hash fileHash, CancellationToken cancellationToken)
    {
        var getResult = await sharedFileRepository.Get(fileHash, cancellationToken);
        if (getResult.TryGetError(out var sharedFile, out var error))
            throw new InvalidOperationException(error.Message);
        
        return new ShardReader(sharedFile, configuration.ShardDirectoryPath);
    }
}