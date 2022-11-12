using System.Security.Cryptography;
using LiteTorrent.Domain;

namespace LiteTorrent.Domain;

public static class Utils
{
    public static byte[] CalculateMerkelTreeRootHash(IEnumerable<Shard> shards)
    {
        // TODO: to full binary tree?
        using var sha256Algorithm = SHA256.Create();
        
        // TODO: 100gb file = 6.25gb hash concat. Too many...
        return sha256Algorithm.ComputeHash(shards.SelectMany(shard => shard.Hash.Data.ToArray()).ToArray());
    }
}