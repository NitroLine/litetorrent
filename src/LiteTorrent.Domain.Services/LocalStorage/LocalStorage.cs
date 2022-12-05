using LiteTorrent.Domain.Services.LocalStorage.HashTrees;
using LiteTorrent.Domain.Services.LocalStorage.Shards;
using LiteTorrent.Domain.Services.LocalStorage.SharedFiles;

namespace LiteTorrent.Domain.Services.LocalStorage;

public record LocalStorage (
    SharedFileRepository SharedFiles,
    ShardRepository Shards,
    HashTreeRepository HashTrees
);