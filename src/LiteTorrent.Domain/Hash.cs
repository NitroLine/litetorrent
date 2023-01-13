using System.Security.Cryptography;
using SimpleBase;

namespace LiteTorrent.Domain;

public readonly struct Hash
{
    // ReSharper disable once MemberCanBePrivate.Global
    public static readonly Hash Empty = new(new byte[32]);

    private readonly byte[] sha256Data;

    // ReSharper disable once UnusedMember.Global
    public Hash()
    {
        sha256Data = Empty.sha256Data;
    }
    
    private Hash(byte[] sha256Data)
    {
        this.sha256Data = sha256Data;
    }

    public bool IsEmpty => this == Empty;

    public ReadOnlyMemory<byte> Data => sha256Data;
    
    public static Hash CreateFromRaw(ReadOnlyMemory<byte> rawData)
    {
        using var algorithm = SHA256.Create();
        return new Hash(algorithm.ComputeHash(rawData.ToArray()));
    }
    
    public static Hash CreateFromSha256Unsafe(byte[] sha256Data)
    {
        if (sha256Data.Length != 32)
            throw new ArgumentException($"Sha256 has 32 bytes, but was {sha256Data.Length}");
            
        return new Hash(sha256Data);
    }
    
    public static bool operator ==(Hash hash1, Hash hash2)
    {
        return hash1.sha256Data == hash2.sha256Data || hash1.sha256Data.SequenceEqual(hash2.sha256Data);
    }

    public static bool operator !=(Hash hash1, Hash hash2)
    {
        return !(hash1 == hash2);
    }
    
    public static Hash[] CreateArray(int count)
    {
        return Enumerable.Repeat(Empty, count).ToArray();
    }

    public Hash Concat(Hash hash2)
    {
        return CreateFromRaw(sha256Data.Concat(hash2.sha256Data).ToArray());
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public bool Equals(Hash other)
    {
        return sha256Data.Equals(other.sha256Data);
    }

    public override bool Equals(object? obj)
    {
        return obj is Hash other && Equals(other);
    }

    public override int GetHashCode()
    {
        return sha256Data.GetHashCode();
    }

    public override string ToString()
    {
        return Base32.Rfc4648.Encode(sha256Data);
    }
}