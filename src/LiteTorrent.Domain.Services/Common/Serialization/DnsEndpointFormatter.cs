using System.Net;
using MessagePack;
using MessagePack.Formatters;

namespace LiteTorrent.Domain.Services.Common.Serialization;

public class DnsEndpointFormatter : IMessagePackFormatter<DnsEndPoint>
{
    public void Serialize(ref MessagePackWriter writer, DnsEndPoint value, MessagePackSerializerOptions options)
    {
        options.Resolver.GetFormatter<string>().Serialize(ref writer, $"{value.Host}:{value.Port}", options);
    }

    public DnsEndPoint Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        var hostPortPair = options.Resolver.GetFormatter<string>().Deserialize(ref reader, options).Split(':');
        return new DnsEndPoint(hostPortPair[0], int.Parse(hostPortPair[1]));
    }
}