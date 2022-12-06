using LiteTorrent.Domain.Services.InterProcessProtocol.Server.Commands;
using MessagePack;

namespace LiteTorrent.Domain.Services.InterProcessProtocol.Server.Serialization;

public static class CommandSerializer
{
    public static byte[] Serialize<TCommand>(TCommand command)
    {
        // TODO: to client
        return command switch
        {
            CreateSharedFileCommand c => Serialize(c),
            AddSharedFileCommand c => Serialize(c),
            _ => throw new NotSupportedException($"{typeof(TCommand)}")
        };
    }

    public static TCommand Deserialize<TCommand>(byte[] commandBytes)
    {
        
    }

    private static byte[] Serialize(CreateSharedFileCommand command)
    {
        var type = BitConverter.GetBytes((int)CommandType.CreateSharedFile);
        var commandBytes = MessagePackSerializer.Serialize(command, SerializerHelper.SerializerOptions);

        return BitConverter.GetBytes(type.Length + commandBytes.Length)
            .Concat(type)
            .Concat(commandBytes)
            .ToArray();
    }
    
    private static byte[] Serialize(AddSharedFileCommand command)
    {
        
    }
}