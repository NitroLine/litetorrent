using MessagePack;

namespace LiteTorrent.Domain.Services.ShardExchange.Messages;

[MessagePackObject]
public record HandshakeAckMessage(
    [property: Key(0)] string PeerId,
    [property: Key(1)] Hash FileHash
);