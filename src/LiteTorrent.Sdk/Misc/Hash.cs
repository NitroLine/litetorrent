namespace LiteTorrent.Sdk.Misc;

public readonly struct Hash
{
    // ReSharper disable once MemberCanBePrivate.Global
    public byte[] Value { get; }

    // ReSharper disable once MemberCanBePrivate.Global
    public Hash(byte[] value)
    {
        Value = value;
    }

    public static bool operator ==(Hash hash1, Hash hash2)
    {
        return hash1.Value.SequenceEqual(hash2.Value);
    }

    public static bool operator !=(Hash hash1, Hash hash2)
    {
        return !(hash1 == hash2);
    }
    
    public override bool Equals(object? obj)
    {
        return obj is Hash hash && hash.Value == Value;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public static implicit operator Hash(byte[] value) => new(value);
    
    public static implicit operator byte[](Hash hash) => hash.Value;
}