using System.Diagnostics.CodeAnalysis;

namespace LiteTorrent.Infra;

public readonly struct Result<TValue>
{
    private readonly TValue? value;
    private readonly Error? error;

    private Result(TValue? value, Error? error)
    {
        this.value = value;
        this.error = error;
    }

    public static implicit operator Result<TValue>(TValue value) => new(value, null);
    public static implicit operator Result<TValue>(Error value) => new(default, value);

    public bool TryGetError([NotNullWhen(false)] out TValue? outValue, [NotNullWhen(true)] out Error? outError)
    {
        outValue = value;
        outError = error;
        
        return outError is not null;
    }
}