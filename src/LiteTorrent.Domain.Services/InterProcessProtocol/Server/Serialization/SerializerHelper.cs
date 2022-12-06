using LiteTorrent.Domain.Services.Common.Serialization;
using MessagePack;
using MessagePack.Resolvers;

namespace LiteTorrent.Domain.Services.InterProcessProtocol.Server.Serialization;

public static class SerializerHelper
{
    public static readonly MessagePackSerializerOptions SerializerOptions = new(
        CompositeResolver.Create(
            StandardResolver.Instance,
            HashResolver.Instance, 
            DnsEndpointResolver.Instance));
}