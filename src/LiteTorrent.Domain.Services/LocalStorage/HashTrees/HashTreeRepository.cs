using LiteTorrent.Core;
using LiteTorrent.Domain.Services.LocalStorage.Common;
using LiteTorrent.Domain.Services.LocalStorage.Configuration;
using LiteTorrent.Domain.Services.LocalStorage.Serialization;
using MessagePack;
using SimpleBase;

namespace LiteTorrent.Domain.Services.LocalStorage.HashTrees;

public class HashTreeRepository
{
    private static readonly MessagePackSerializerOptions Options = LocalStorageHelper.SerializerOptions;
    
    private readonly LocalStorageConfiguration configuration;

    public HashTreeRepository(LocalStorageConfiguration configuration)
    {
        this.configuration = configuration;
    }
    
    /// <summary>
    /// Saving in file which name is correlated shared file hash in base32
    /// </summary>
    public async Task<Result<Unit>> CreateOrReplace(MerkleTree merkleTree)
    {
        await using var fileLock = await LocalStorageHelper.FilePool.GetToWrite(GetFileName(merkleTree.RootHash));

        var (trees, rootTree, rootHash, pieces) = merkleTree.GetInnerData();

        await MessagePackSerializer.SerializeAsync(
            fileLock.FileStream,
            new DtoHashTree(trees, rootTree, rootHash, pieces),
            Options);
        
        await fileLock.FileStream.FlushAsync();

        return Result.Ok;
    }
    
    /// <param name="hash">Correlated shared file hash</param>
    public async Task<Result<MerkleTree>> Get(Hash hash)
    {
        await using var fileLock = await LocalStorageHelper.FilePool.GetToRead(GetFileName(hash));

        var dto = await MessagePackSerializer.DeserializeAsync<DtoHashTree>(fileLock.FileStream, Options);

        return new MerkleTree(dto.Trees, dto.RootTree, dto.RootHash, dto.Pieces);
    }

    private string GetFileName(Hash rootHash)
    {
        return configuration.InHashTreeDir(Base32.Rfc4648.Encode(rootHash.Data.Span));
    }
}