namespace LiteTorrent.Domain.Services.PieceExchange.Messages;

public enum MessageOpcode
{
    HandshakeInit,
    HandshakeAck,
    Bitfield,
    PieceRequest,
    PieceResponse
}