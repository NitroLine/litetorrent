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
        await using var file = GetFileStream(merkleTree.RootHash);
        
        var (trees, rootTree, rootHash, pieces) = merkleTree.GetInnerData();

        await MessagePackSerializer.SerializeAsync(
            file,
            new DtoHashTree(trees, rootTree, rootHash, pieces),
            Options);

        return Result.Ok;
    }
    
    /// <param name="hash">Correlated shared file hash</param>
    public async Task<Result<MerkleTree>> Get(Hash hash)
    {
        await using var file = GetFileStream(hash);

        var dto = await MessagePackSerializer.DeserializeAsync<DtoHashTree>(file, Options);

        return new MerkleTree(dto.Trees, dto.RootTree, dto.RootHash, dto.Pieces);
    }

    private FileStream GetFileStream(Hash rootHash)
    {
        var path = Path.Join(configuration.HashTreeDirectoryPath, Base32.Rfc4648.Encode(rootHash.Data.Span));
        return LocalStorageHelper.GetFileStreamToWrite(path);
    }
}