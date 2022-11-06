namespace LiteTorrent.Domain.Services.LocalStorage.SharedFiles;

public interface ISharedFileRepository
{
    Task<Hash> Save(SharedFileBuildInfo buildInfo, CancellationToken cancellationToken);
    Task<SharedFile> Get(Hash hash, CancellationToken cancellationToken);
}