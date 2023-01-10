using LiteTorrent.Core;
using LiteTorrent.Domain.Services.LocalStorage.Common;
using LiteTorrent.Domain.Services.LocalStorage.Configuration;
using LiteTorrent.Domain.Services.LocalStorage.HashTrees;
using LiteTorrent.Domain.Services.LocalStorage.Serialization;
using MessagePack;
using SimpleBase;

namespace LiteTorrent.Domain.Services.LocalStorage.SharedFiles;

public class SharedFileRepository
{
    private readonly HashTreeRepository hashTreeRepository;
    private readonly LocalStorageConfiguration configuration;

    public SharedFileRepository(
        HashTreeRepository hashTreeRepository, 
        LocalStorageConfiguration configuration)
    {
        this.hashTreeRepository = hashTreeRepository;
        this.configuration = configuration;
    }

    /// <summary>
    /// Can be used for seeding
    /// </summary>
    public async Task<Result<Hash>> Create(
        SharedFileCreateInfo sharedFileInfo, 
        CancellationToken cancellationToken)
    {
        var rawFileInfo = new FileInfo(configuration.InPieceDir(sharedFileInfo.RelativePath));
        await using var dataStreamLock = await LocalStorageHelper.FilePool.GetToRead(rawFileInfo.FullName);
        
        var shardHashes = await LocalStorageHelper
            .SplitData(dataStreamLock.FileStream, sharedFileInfo.ShardMaxSizeInBytes, cancellationToken)
            .Select(shard => Hash.CreateFromRaw(shard.Data))
            .ToArrayAsync(cancellationToken);

        var hashTree = new MerkleTree(shardHashes);
        var createResult = await hashTreeRepository.CreateOrReplace(hashTree);
        if (createResult.TryGetError(out _, out var error))
            return error;

        var sharedFilePath = configuration.InSharedFileDir(GetFileName(hashTree.RootHash));
        var dto = await SaveSharedFileInfo(sharedFilePath, rawFileInfo.Length, sharedFileInfo, cancellationToken);

        return hashTree.RootHash;
    }
    
    /// <summary>
    /// Save in storage with id = hash.
    /// Useful for save shared file that was downloaded from remote sources.   
    /// </summary>
    public async Task<Result<Unit>> Save(
        Hash hash,
        long sizeInBytes,
        SharedFileCreateInfo createInfo, 
        CancellationToken cancellationToken)
    {
        var dto = await SaveSharedFileInfo(
            Path.Join(configuration.SharedFileDirectoryPath, GetFileName(hash)),
            sizeInBytes,
            createInfo,
            cancellationToken);

        var hashTree = new MerkleTree((int)(dto.SizeInBytes / dto.ShardMaxSizeInBytes), hash);
        var saveResult = await hashTreeRepository.CreateOrReplace(hashTree);
        
        return saveResult.TryGetError(out _, out var error) ? error : Result.Ok;
    }

    public async Task<Result<SharedFile>> Get(Hash hash, CancellationToken cancellationToken)
    {
        await using var fileLock = await LocalStorageHelper.FilePool.GetToRead(
            LocalStorageHelper.GetFilePath(configuration.SharedFileDirectoryPath, hash));

        return await InnerGet(fileLock.FileStream, hash, cancellationToken);
    }

    public async Task<Result<List<SharedFile>>> GetAll(CancellationToken cancellationToken)
    {
        var result = new List<SharedFile>();
        foreach (var fileName in Directory.EnumerateFiles(configuration.SharedFileDirectoryPath))
        {
            await using var fileLock = await LocalStorageHelper.FilePool.GetToRead(fileName);

            var getResult = await InnerGet(
                fileLock.FileStream, 
                GetHashFromFileName(fileName),
                cancellationToken);

            if (getResult.TryGetError(out var sharedFile, out var error))
                return error;
            
            result.Add(sharedFile);
        }

        return result;
    }
    
    private static Hash GetHashFromFileName(string fileFullName)
    {
        return Hash.CreateFromSha256(Base32.Rfc4648.Decode(new FileInfo(fileFullName).Name));
    }
    
    private static string GetFileName(Hash hash) => Base32.Rfc4648.Encode(hash.Data.Span);

    private static async Task<DtoSharedFile> SaveSharedFileInfo(
        string fileName,
        long sizeInBytes,
        SharedFileCreateInfo createInfo,
        CancellationToken cancellationToken)
    {
        var dto = new DtoSharedFile(
            createInfo.RelativePath, 
            (ulong)sizeInBytes,
            createInfo.ShardMaxSizeInBytes);

        await using var fileLock = await LocalStorageHelper.FilePool.GetToWrite(fileName);
        await MessagePackSerializer.SerializeAsync(
            fileLock.FileStream,
            dto,
            LocalStorageHelper.SerializerOptions, 
            cancellationToken);

        return dto;
    }

    private async Task<Result<SharedFile>> InnerGet(Stream file, Hash hash, CancellationToken cancellationToken)
    {
        var dto = await MessagePackSerializer.DeserializeAsync<DtoSharedFile>(
            file, 
            LocalStorageHelper.SerializerOptions, 
            cancellationToken);

        var getHashTreeResult = await hashTreeRepository.Get(hash);
            
        if (getHashTreeResult.TryGetError(out var hashTree, out var errorInfo))
            return errorInfo;

        return new SharedFile(hashTree, dto.RelativePath, dto.SizeInBytes, dto.ShardMaxSizeInBytes);
    }
}