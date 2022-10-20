using System.Net;
using System.Net.WebSockets;

namespace NatPuncher.Client;

public interface INatPuncherClient
{
    public Task<IPEndPoint> PunchNat(
        IPEndPoint puncherServerAddress,
        IPEndPoint bindAddress,
        CancellationToken cancellationToken);
    public Task<WebSocket> AcceptConnection(CancellationToken cancellationToken);
}