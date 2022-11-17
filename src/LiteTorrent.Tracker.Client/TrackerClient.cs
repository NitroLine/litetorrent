using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Web;
using LiteTorrent.Infra;
using Newtonsoft.Json;

namespace LiteTorrent.Tracker.Client;

public class TrackerClient : ITrackerClient
{
    private readonly bool _raiseException;
    private readonly HttpClient _client = new HttpClient();

    public TrackerClient(Uri trackerUri, bool raiseException = true)
    {
        _raiseException = raiseException;
        TrackerUri = trackerUri;
    }

    public Guid Uid { get; private set; }
    public Uri TrackerUri { get; private set; }
    
    public async Task Register(IPEndPoint clientEndpoint)
    {
        var peersUrl = TrackerUri.AbsoluteUri + "peers";
        var data = new { publicAddress = new
        {
            ip = clientEndpoint.Address.ToString(),
            port = clientEndpoint.Port
        } 
        };
        var response = await _client.PostAsJsonAsync(peersUrl, data); 
        
        response.EnsureSuccessStatusCode();
        var jsonString = await response.Content.ReadAsStringAsync();
        var content = JsonConvert.DeserializeObject<RegisterResponse>(jsonString);
        Debug.Assert(content != null, nameof(content) + " != null");
        Uid = content.id;
    }

    public async Task Unregister()
    {
        var peersUrl = TrackerUri.AbsoluteUri + "peers/" + Uid;
        var response = await _client.DeleteAsync(peersUrl); 
        if (_raiseException)
            response.EnsureSuccessStatusCode();
    }

    public async Task Update(IReadOnlyList<Hash> shareFilesIds)
    {
        var peersUrl = TrackerUri.AbsoluteUri + "peers/" + Uid;
        var data = new { distributingFiles = shareFilesIds.Select((x) => x.ToString()).ToArray() };
        var response = await _client.PutAsJsonAsync(peersUrl, data); 
        if (_raiseException)
         response.EnsureSuccessStatusCode();
    }

    public async Task<IEnumerable<Peer>> GetPeers(Hash fileId)
    {
        var builder = new UriBuilder(TrackerUri.AbsoluteUri + "peers");
        // builder.Port = -1;
        var query = HttpUtility.ParseQueryString(builder.Query);
        query["field"] = fileId.ToString();
        builder.Query = query.ToString();
        var peersUrl = builder.ToString();
        var response = await _client.GetAsync(peersUrl);
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