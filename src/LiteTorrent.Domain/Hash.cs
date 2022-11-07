using System.Security.Cryptography;

namespace LiteTorrent.Domain;

public readonly struct Hash
{
    private readonly byte[] sha256Data;

    private Hash(byte[] sha256Data)
    {
        this.sha256Data = sha256Data;
    }

    public ReadOnlyMemory<byte> Data => sha256Data;

    // TODO: Result<Hash>
    public static Hash CreateFromRaw(ReadOnlyMemory<byte> rawData)
    {
        using var algorithm = SHA256.Create();
        return new Hash(algorithm.ComputeHash(rawData.ToArray()));
    }
    
    public static bool operator ==(Hash hash1, Hash hash2)
    {
        return hash1.sha256Data.SequenceEqual(hash2.sha256Data);
    }

    public static bool operator !=(Hash hash1, Hash hash2)
    {
        return !(hash1 == hash2);
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
        return Convert.ToHexString(sha256Data);
    }
}