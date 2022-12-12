using System.Collections;
using MessagePack;

namespace LiteTorrent.Domain.Services.PieceExchange.Messages;

[MessagePackObject]
public record BitfieldMessage (
    [property: Key(0)] BitArray Mask
);
