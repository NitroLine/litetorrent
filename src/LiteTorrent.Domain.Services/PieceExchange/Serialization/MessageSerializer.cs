using LiteTorrent.Domain.Services.Common.Serialization;
using LiteTorrent.Domain.Services.PieceExchange.Messages;
using LiteTorrent.Domain.Services.ShardExchange.Messages;
using MessagePack;

namespace LiteTorrent.Domain.Services.PieceExchange.Serialization;

public static class MessageSerializer
{
    private static readonly Dictionary<MessageOpcode, Type> TypeByOpcode = new()
    {
        { MessageOpcode.HandshakeInit, typeof(HandshakeInitMessage) },
        { MessageOpcode.HandshakeAck, typeof(HandshakeAckMessage) },
        { MessageOpcode.Bitfield, typeof(BitfieldMessage) },
        { MessageOpcode.PieceRequest, typeof(PieceRequestMessage) },
        { MessageOpcode.PieceResponse, typeof(PieceResponseMessage) }
    };
    
    private static readonly Dictionary<Type, MessageOpcode> OpcodeByType = new();

    static MessageSerializer()
    {
        foreach (var (key, value) in TypeByOpcode)
            OpcodeByType[value] = key;
    }
    
    public static byte[] Serialize(object payload)
    {
        var payloadBytes = MessagePackSerializer.Serialize(payload, SerializerHelper.DefaultOptions);

        return BitConverter
            .GetBytes((int)OpcodeByType[payload.GetType()])
            .Concat(payloadBytes)
            .ToArray();
    }

    public static object Deserialize(ReadOnlyMemory<byte> rawMessage)
    {
        var opcode = (MessageOpcode)BitConverter.ToInt32(rawMessage[..4].Span);
        var payload = MessagePackSerializer.Deserialize(
            TypeByOpcode[opcode],
            rawMessage[4..],
            SerializerHelper.DefaultOptions);

        return payload;
    }
}