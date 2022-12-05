using MessagePack;
using MessagePack.Formatters;

namespace LiteTorrent.Domain.Services.Serialization;

public class HashResolver : IFormatterResolver
{
    public static HashResolver Instance = new(); 
    
    public IMessagePackFormatter<T>? GetFormatter<T>()
    {
        // TODO: Cache<T>
        return typeof(T) != typeof(Hash) ? null : (IMessagePackFormatter<T>)new HashFormatter();
    }
}