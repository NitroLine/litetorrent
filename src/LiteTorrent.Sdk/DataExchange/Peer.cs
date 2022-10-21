using System.Net.Sockets;
using System.Net.WebSockets;
using LiteTorrent.Sdk.Sharding;

namespace LiteTorrent.Sdk.DataExchange;

public class Peer
{
    private readonly WebSocket connection;
    private readonly IReadOnlyList<ShardedFile> requiredFiles;
    private readonly IReadOnlyList<ShardedFile> peerDistributingFiles;

    public Peer(
        WebSocket connection,
        IReadOnlyList<ShardedFile> requiredFiles, 
        IReadOnlyList<ShardedFile> peerDistributingFiles)
    {
        this.connection = connection;
        this.requiredFiles = requiredFiles;
        this.peerDistributingFiles = peerDistributingFiles;
    }

    public static Peer Create(Socket connection, Distribution distribution)
    {
        var webSocketConnection = WebSocket.CreateFromStream(
            new NetworkStream(connection), 
            true,
            null, 
            TimeSpan.FromMinutes(2));
        
        // Receive requiredFiles and peerDistributingFiles
        // Send distribution data
    }
}