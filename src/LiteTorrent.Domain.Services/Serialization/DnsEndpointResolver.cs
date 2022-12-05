using System.Net;
using MessagePack;
using MessagePack.Formatters;

namespace LiteTorrent.Domain.Services.Serialization;

public class DnsEndpointResolver : IFormatterResolver
{
    public static readonly DnsEndpointResolver Instance = new();
    
    public IMessagePackFormatter<T> GetFormatter<T>()
    {
        // TODO: Cache<T> or Formatter.Instance?
        return typeof(T) == typeof(DnsEndPoint) ? (IMessagePackFormatter<T>)new DnsEndpointFormatter() : null!;
    }
}