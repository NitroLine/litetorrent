using System.Net;

namespace LiteTorrent.Tracker.Client.Domain;

public record Peer(Guid Id, IPAddress Ip, int Port);

