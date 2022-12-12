namespace LiteTorrent.Domain.Services.ShardExchange.Messages;

public enum MessageOpcode
{
    HandshakeInit,
    HandshakeAck,
    Bitfield,
    ShardRequest,
    ShardResponse
}