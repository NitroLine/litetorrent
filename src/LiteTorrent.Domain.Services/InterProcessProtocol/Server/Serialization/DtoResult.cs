namespace LiteTorrent.Domain.Services.InterProcessProtocol.Server.Serialization;

public record DtoResult(bool IsOk, byte[] Payload);