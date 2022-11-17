using LiteTorrent.Domain.Services.LocalStorage.Configuration;
using LiteTorrent.Infra;

namespace LiteTorrent.Domain.Services.LocalStorage.SharedFiles;

public class SharedFileRepository : ISharedFileRepository
{
    private readonly SqliteConfiguration configuration;

    public SharedFileRepository(SqliteConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public Task<Hash> Save(SharedFileBuildInfo buildInfo, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<SharedFile> Get(Hash hash, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}