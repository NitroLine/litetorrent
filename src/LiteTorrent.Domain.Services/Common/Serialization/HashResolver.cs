using MessagePack;
using MessagePack.Formatters;

namespace LiteTorrent.Domain.Services.Common.Serialization;

public class HashResolver : IFormatterResolver
{
    public static readonly HashResolver Instance = new(); 
    
    public IMessagePackFormatter<T>? GetFormatter<T>()
    {
        // TODO: Cache<T>
        return typeof(T) != typeof(Hash) ? null : (IMessagePackFormatter<T>)new HashFormatter();
    }
}