using System.Net;

namespace LiteTorrent.Domain.Services.PieceExchange;

public record DownloadingConfiguration(DnsEndPoint[] AllowedPeers);