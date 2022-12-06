using LiteTorrent.Domain.Services.InterProcessProtocol.Server.Commands;

namespace LiteTorrent.Domain.Services.InterProcessProtocol.Server.Serialization;

// useless
public record DtoCommand(
    CommandType Type, 
    byte[] Payload
);
