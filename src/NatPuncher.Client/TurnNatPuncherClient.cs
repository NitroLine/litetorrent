using System.Net;
using System.Net.WebSockets;

namespace NatPuncher.Client;

public class TurnNatPuncherClient : INatPuncherClient
{
    public Task<IPEndPoint> PunchNat(
        IPEndPoint puncherServerAddress, 
        IPEndPoint bindAddress, 
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<WebSocket> AcceptConnection(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}