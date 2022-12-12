using System.Collections;

namespace LiteTorrent.Domain.Services.PieceExchange.Messages;

public record ConnectionContext(SharedFile SharedFile, BitArray OtherBitfield);