// See https://aka.ms/new-console-template for more information

using System.Net;
using System.Text;
using LiteTorrent.Infra;
using LiteTorrent.Tracker.Client;

Console.WriteLine("Hello, World!");

var my_hash = "ababa";
var hash = Hash.CreateFromRaw(new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(my_hash)));
var uri = new Uri("http://localhost:3338");
Console.WriteLine(uri.AbsoluteUri);
var client = new TrackerClient(uri);
await client.Register(IPEndPoint.Parse("127.0.0.1:3547"));
await client.Update(new List<Hash>() {hash});
var result = await client.GetPeers(hash);
foreach (var peer in result)
{
    Console.WriteLine(peer.EndPont.Address.ToString());
}
await client.Unregister();
Console.WriteLine("done");