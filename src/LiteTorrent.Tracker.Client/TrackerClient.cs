using LiteTorrent.Domain;
using LiteTorrent.Tracker.Client.Domain;

namespace LiteTorrent.Tracker.Client;

public class TrackerClient : ITrackerClient
{
    private static readonly HttpClient client = new HttpClient();

    public TrackerClient(Uri trackerUri)
    {
        TrackerUri = trackerUri;
    }

    public Guid Uid { get; private set; }
    public Uri TrackerUri { get; private set; }
    
    public async Task Register(IReadOnlyList<Hash> shareFilesIds)
    {
        var peersUrl = TrackerUri.AbsolutePath + "/peers";
        var body = new Dictionary<string, string>
        {
            { "thing1", "hello" },
            { "thing2", "world" }
        };
        var content = new FormUrlEncodedContent(body);
        var responce = await client.PostAsync(peersUrl, content); 
        Uid = Guid.Parse(responce.Content.ToString() ?? string.Empty);
    }

    public Task Unregister()
    {
        throw new NotImplementedException();
    }

    public Task Update(IReadOnlyList<Hash> shareFilesIds)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Peer>> GetPeers(Hash fileId)
    {
        throw new NotImplementedException();
    }
}