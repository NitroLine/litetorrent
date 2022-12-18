using System.Net;

public record TransportConfiguration(IPEndPoint TorrentEndpoint, string PeerId);