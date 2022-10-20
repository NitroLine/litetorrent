using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text.Json;
using NatPuncher.Client.Misc;

namespace NatPuncher.Client.Tests;

public static class EntryPoint
{
    public static async Task Main()
    {
        var puncherSocket = SocketHelper.CreateSocketWithAddressReusing();
        var distributionSocket = SocketHelper.CreateSocketWithAddressReusing();

        await puncherSocket.ConnectAsync("localhost", 7000);
        Console.WriteLine("Connected");

        var puncherWebSocket = WebSocket.CreateFromStream(
            new NetworkStream(puncherSocket),
            false,
            null,
            TimeSpan.FromMinutes(2));

        var buffer = new byte[65536];
        var result = await puncherWebSocket.ReceiveAsync(buffer, CancellationToken.None);

        var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(
            new ReadOnlySpan<byte>(buffer, 0, result.Count))!;
        var ip = dict["ip"];
        var port = dict["port"];

        Console.WriteLine($"Public address: {ip}:{port}");
    }
}