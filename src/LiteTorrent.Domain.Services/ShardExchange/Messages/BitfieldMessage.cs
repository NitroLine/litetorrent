using System.Collections;
using MessagePack;

namespace LiteTorrent.Domain.Services.ShardExchange.Messages;

[MessagePackObject]
public record BitfieldMessage (
    [property: Key(0)] BitArray Mask
);
