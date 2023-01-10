using LiteTorrent.Domain.Services.LocalStorage.Configuration;
using LiteTorrent.Domain.Services.LocalStorage.SharedFiles;

namespace LiteTorrent.Domain.Services.LocalStorage.Pieces;

public class PieceRepository
{
    private readonly SharedFileRepository sharedFileRepository;
    private readonly LocalStorageConfiguration configuration;

    public PieceRepository(
        SharedFileRepository sharedFileRepository, 
        LocalStorageConfiguration configuration)
    {
        this.sharedFileRepository = sharedFileRepository;
        this.configuration = configuration;
    }

    public async Task<PieceWriter> CreateWriter(Hash fileHash, CancellationToken cancellationToken)
    {
        var getResult = await sharedFileRepository.Get(fileHash, cancellationToken);
        if (getResult.TryGetError(out var sharedFile, out var error))
            throw new InvalidOperationException(error.Message);
        
        return new PieceWriter(sharedFile, configuration.PieceDirectoryPath);
    }

    public async Task<PieceReader> CreateReader(Hash fileHash, CancellationToken cancellationToken)
    {
        var getResult = await sharedFileRepository.Get(fileHash, cancellationToken);
        if (getResult.TryGetError(out var sharedFile, out var error))
            throw new InvalidOperationException(error.Message);
        
        return new PieceReader(sharedFile, configuration.PieceDirectoryPath);
    }
}