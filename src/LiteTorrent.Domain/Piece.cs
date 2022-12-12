namespace LiteTorrent.Domain;

public record struct Piece(ulong Index, ReadOnlyMemory<byte> Data);