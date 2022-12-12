// See https://aka.ms/new-console-template for more information

using System.Text;
using LiteTorrent.Domain;

Hash GetHash(string s)
{
    return Hash.CreateFromRaw(new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(s)));
}

var merkleTree = new MerkleTree(13, GetHash("1234").Concat(GetHash("5")).Concat(GetHash("6")).Concat(GetHash("7")).Concat(GetHash("8")));

var a = merkleTree.TryAdd(9, GetHash("6"), new[] { GetHash("5"), GetHash("7"), GetHash("8"), GetHash("1234") });
Console.WriteLine(a);
var data = new[]
    { GetHash("5"), GetHash("7"), GetHash("8"), GetHash("1"), GetHash("2"), GetHash("3"), GetHash("4"), GetHash("23") };
var buildedTree = new MerkleTree(data);
Console.WriteLine(buildedTree.RootHash);
var rootHash = buildedTree.RootHash;
foreach (var hash in buildedTree.GetPath(1))
{
    Console.WriteLine(hash);
}

var path = buildedTree.GetPath(2);
var newTree = new MerkleTree(data.Length, rootHash);
Console.WriteLine(newTree.TryAdd(2, data[2], path.ToArray()));

