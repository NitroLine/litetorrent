using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text.Json;
using NatPuncher.Client.Misc;

namespace NatPuncher.Client;

public class EchoNatPuncherClient : INatPuncherClient
{
    private readonly Socket puncherSocket;
    private readonly Socket distributionSocket;
    
    private WebSocket? puncherWebSocket;

    public EchoNatPuncherClient()
    {
        puncherSocket = SocketHelper.CreateSocketWithAddressReusing();
        distributionSocket = SocketHelper.CreateSocketWithAddressReusing();
    }
    
    public async Task<IPEndPoint> PunchNat(
        IPEndPoint puncherServerAddress, 
        IPEndPoint bindAddress, 
        CancellationToken cancellationToken)
    {
        puncherSocket.Bind(bindAddress);
        await puncherSocket.ConnectAsync(puncherServerAddress, cancellationToken);
        puncherWebSocket = CreateWebSocket(puncherSocket);

        var publicAddress = await GetPublicAddress(puncherWebSocket, cancellationToken);

        distributionSocket.Bind(bindAddress);
        distributionSocket.Listen();

        return publicAddress;
    }

    public async Task<WebSocket> AcceptConnection(CancellationToken cancellationToken)
    {
        return CreateWebSocket(await distributionSocket.AcceptAsync(cancellationToken));
    }

    private static async Task<IPEndPoint> GetPublicAddress(WebSocket webSocket, CancellationToken cancellationToken)
    {
        var buffer = new byte[1024];
        var result = await webSocket.ReceiveAsync(buffer, cancellationToken);
        var dtoPublicAddress = JsonSerializer.Deserialize<Dictionary<string, string>>(
            new ReadOnlySpan<byte>(buffer, 0, result.Count))!;

        return new IPEndPoint(
            IPAddress.Parse(dtoPublicAddress["ip"]), 
            int.Parse(dtoPublicAddress["port"]));
    }

    private static WebSocket CreateWebSocket(Socket socket)
    {
        return WebSocket.CreateFromStream(
            new NetworkStream(socket), 
            false, 
            null,
            TimeSpan.FromMinutes(2));
    }
    
    ~EchoNatPuncherClient()
    {
        puncherWebSocket?.Dispose();
        distributionSocket.Dispose();
    }
}