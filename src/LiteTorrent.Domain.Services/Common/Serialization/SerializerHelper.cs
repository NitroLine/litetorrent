using LiteTorrent.Core;
using MessagePack;
using MessagePack.Resolvers;

namespace LiteTorrent.Domain.Services.Common.Serialization;

public static class SerializerHelper
{
    public static readonly MessagePackSerializerOptions DefaultOptions = new(
        CompositeResolver.Create(
            StandardResolver.Instance,
            HashResolver.Instance, 
            DnsEndpointResolver.Instance));

    public static byte[] Serialize(this Error error)
    {
        return MessagePackSerializer.Serialize(error.Message, DefaultOptions);
    }
}