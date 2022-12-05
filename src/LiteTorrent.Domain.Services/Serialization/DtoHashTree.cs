using MessagePack;

namespace LiteTorrent.Domain.Services.Serialization;

[MessagePackObject]
public record DtoHashTree(
    [property: Key(0)] List<Hash[]> Trees,
    [property: Key(1)] Hash[] RootTree,
    [property: Key(2)] Hash RootHash
);