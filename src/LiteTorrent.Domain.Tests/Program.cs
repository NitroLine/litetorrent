// See https://aka.ms/new-console-template for more information

using System.Text;
using LiteTorrent.Domain;

Hash GetHash(string s)
{
    return Hash.CreateFromRaw(new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(s)));
}

var merkleTree = new MerkelTree(13, GetHash("1234").Concat(GetHash("5")).Concat(GetHash("6")).Concat(GetHash("7")).Concat(GetHash("8")));

var a = merkleTree.TryAdd(9, GetHash("6"), new[] { GetHash("5"), GetHash("7"), GetHash("8"), GetHash("1234") });
Console.WriteLine(a);

var buildedTree = new MerkelTree(new[] { GetHash("5"), GetHash("7"), GetHash("8"), GetHash("1"), GetHash("2"), GetHash("3"), GetHash("4"), GetHash("23") });
Console.WriteLine(buildedTree.RootHash);

