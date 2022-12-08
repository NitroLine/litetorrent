using System.Net;

namespace LiteTorrent.Domain.Services.ShardExchange;

public record DownloadingConfiguration(DnsEndPoint[] AllowedPeers);