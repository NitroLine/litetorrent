using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Web;
using LiteTorrent.Domain;
using LiteTorrent.Tracker.Client.Domain;
using Newtonsoft.Json;

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
    
    public async Task Register(IPEndPoint? clientEndpoint, IReadOnlyList<Hash> shareFilesIds)
    {
        var peersUrl = TrackerUri.AbsolutePath + "/peers";
        var data = new { publicAddress = new
        {
            ip = clientEndpoint.Address.ToString(),
            port = clientEndpoint.Port
        }, distributingFiles = shareFilesIds.Select((x) => x.ToString()).ToArray() };
        var response = await client.PostAsJsonAsync(peersUrl, data); 
        response.EnsureSuccessStatusCode();
        var jsonString = await response.Content.ReadAsStringAsync();
        var content = JsonConvert.DeserializeObject<RegisterResponse>(jsonString);
        Debug.Assert(content != null, nameof(content) + " != null");
        Uid = content.id;
    }

    public async Task Unregister()
    {
        var peersUrl = TrackerUri.AbsolutePath + "/peers/" + Uid;
        var response = await client.DeleteAsync(peersUrl); 
        // response.EnsureSuccessStatusCode();
    }

    public async Task Update(IReadOnlyList<Hash> shareFilesIds)
    {
        var peersUrl = TrackerUri.AbsolutePath + "/peers/" + Uid;
        var data = new { distributingFiles = shareFilesIds.Select((x) => x.ToString()).ToArray() };
        var response = await client.PutAsJsonAsync(peersUrl, data); 
        // response.EnsureSuccessStatusCode();
    }

    public async Task<IEnumerable<Peer>> GetPeers(Hash fileId)
    {
        var builder = new UriBuilder(TrackerUri.AbsolutePath);
        builder.Port = -1;
        var query = HttpUtility.ParseQueryString(builder.Query);
        query["fileId"] = fileId.ToString();
        builder.Query = query.ToString();
        var peersUrl = builder.ToString();
        var response = await client.GetAsync(peersUrl); 
        if (!response.IsSuccessStatusCode)
            return Enumerable.Empty<Peer>();
        var jsonString = await response.Content.ReadAsStringAsync();
        var content = JsonConvert.DeserializeObject<ListPeersResponse>(jsonString);
        if (content is null)
            return Enumerable.Empty<Peer>();
        return content.peerIds.Select((peer) =>
        {
            try
            {
                var endpoint = new IPEndPoint(IPAddress.Parse(peer.ip), peer.port);
                return new Peer(endpoint);
            }
            catch (Exception err)
            {
                return null;
            }
        }).Where((peer) => peer is not null)!;
    }
}

record RegisterResponse
{
    public Guid id;
}

record ListPeersResponse
{
    public DtoPeer[] peerIds;
}

record DtoPeer
{
    public string ip;
    public int port;
}