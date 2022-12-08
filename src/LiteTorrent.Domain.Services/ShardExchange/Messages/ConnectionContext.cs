using System.Collections;

namespace LiteTorrent.Domain.Services.ShardExchange.Messages;

public record ConnectionContext(SharedFile SharedFile, BitArray OtherBitfield);