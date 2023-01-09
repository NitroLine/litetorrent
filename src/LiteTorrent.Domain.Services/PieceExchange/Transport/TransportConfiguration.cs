using System.Net;

namespace LiteTorrent.Domain.Services.PieceExchange.Transport;

public record TransportConfiguration(IPEndPoint TorrentEndpoint, string PeerId);