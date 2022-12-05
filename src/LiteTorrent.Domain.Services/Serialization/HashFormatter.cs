using MessagePack;
using MessagePack.Formatters;

namespace LiteTorrent.Domain.Services.Serialization;

public class HashFormatter : IMessagePackFormatter<Hash>
{
    public void Serialize(ref MessagePackWriter writer, Hash value, MessagePackSerializerOptions options)
    {
        options.Resolver.GetFormatter<byte[]>().Serialize(ref writer, value.Data.ToArray(), options);
    }

    public Hash Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        return Hash.CreateFromSha256(options.Resolver.GetFormatter<byte[]>().Deserialize(ref reader, options));
    }
}