using System.Net;

public record TransportConfiguration(DnsEndPoint TorrentEndpoint, string PeerId);