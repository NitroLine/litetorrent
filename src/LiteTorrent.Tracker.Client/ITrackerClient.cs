using LiteTorrent.Domain;
using LiteTorrent.Tracker.Client.Domain;

namespace LiteTorrent.Tracker.Client;

public interface ITrackerClient
{
    Task Register(IReadOnlyList<Hash> shareFilesIds);
    Task Unregister();
    Task Update(IReadOnlyList<Hash> shareFilesIds);
    Task<IEnumerable<Peer>> GetPeers(Hash fileId);
}