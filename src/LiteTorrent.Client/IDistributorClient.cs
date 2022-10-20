using System.Net;
using LiteTorrent.Sdk;
using LiteTorrent.Sdk.Sharding;

namespace LiteTorrent.Client;

public interface IDistributorClient
{
    Task Announce(IPEndPoint tracker);
    Task<DistributingFileId> AddToDistribution(ShardedFile file);
    Task DeleteFromDistribution(DistributingFileId id);
    Task<IEnumerable<ShardedFile>> GetAllDistributingFiles();
    Task<Peer> AcceptConnection();
}