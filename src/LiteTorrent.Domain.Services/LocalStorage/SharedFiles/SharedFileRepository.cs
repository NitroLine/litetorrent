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
        var rawFilePath = Path.Join(configuration.SharedFileDirectoryPath, sharedFileInfo.RelativePath);
        await using var dataStream = new FileStream(rawFilePath, FileMode.Open, FileAccess.Read);
        
        var shardHashes = await LocalStorageHelper
            .SplitData(dataStream, sharedFileInfo.ShardMaxSizeInBytes, cancellationToken)
            .Select(shard => Hash.CreateFromRaw(shard.Data))
            .ToArrayAsync(cancellationToken);

        var hashTree = new MerkelTree(shardHashes);
        var createResult = await hashTreeRepository.CreateOrReplace(hashTree);
        if (createResult.TryGetError(out _, out var error))
            return error;

        await using var file = LocalStorageHelper.GetFileStreamToWrite(GetFileName(hashTree.RootHash));
        await SaveSharedFileInfo(file, sharedFileInfo, cancellationToken);

        return hashTree.RootHash;
    }
    
    /// <summary>
    /// Save in storage with id = hash.
    /// Useful for save shared file that was downloaded from remote sources.   
    /// </summary>
    public async Task<Result<Unit>> Create(
        Hash hash, 
        SharedFileCreateInfo createInfo, 
        CancellationToken cancellationToken)
    {
        await using var file = LocalStorageHelper.GetFileStreamToWrite(GetFileName(hash));

        var dto = await SaveSharedFileInfo(file, createInfo, cancellationToken);

        var hashTree = new MerkelTree((int)(dto.SizeInBytes / dto.ShardMaxSizeInBytes), hash);
        var saveResult = await hashTreeRepository.CreateOrReplace(hashTree);
        
        return saveResult.TryGetError(out _, out var error) ? error : Result.Ok;
    }

    public async Task<Result<SharedFile>> Get(Hash hash, CancellationToken cancellationToken)
    {
        await using var file = LocalStorageHelper.GetFileStreamToRead(
            LocalStorageHelper.GetFilePath(configuration.SharedFileDirectoryPath, hash));

        return await InnerGet(file, hash, cancellationToken);
    }

    public async Task<Result<List<SharedFile>>> GetAll(CancellationToken cancellationToken)
    {
        var result = new List<SharedFile>();
        foreach (var fileName in Directory.EnumerateFiles(configuration.SharedFileDirectoryPath))
        {
            await using var file = LocalStorageHelper.GetFileStreamToRead(fileName);

            var getResult = await InnerGet(
                file, 
                GetHashFromFileName(fileName),
                cancellationToken);

            if (getResult.TryGetError(out var sharedFile, out var error))
                return error;
            
            result.Add(sharedFile);
        }

        return result;
    }
    
    private static Hash GetHashFromFileName(string fileName)
    {
        return Hash.CreateFromSha256(Base32.Rfc4648.Decode(fileName));
    }
    
    private static string GetFileName(Hash hash) => Base32.Rfc4648.Encode(hash.Data.Span);

    private static async Task<DtoSharedFile> SaveSharedFileInfo(
        Stream stream,
        SharedFileCreateInfo createInfo,
        CancellationToken cancellationToken)
    {
        var dto = new DtoSharedFile(
            createInfo.Trackers, 
            createInfo.RelativePath, 
            createInfo.SizeInBytes,
            createInfo.ShardMaxSizeInBytes);

        await MessagePackSerializer.SerializeAsync(
            stream, 
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

        return new SharedFile(hashTree, dto.Trackers, dto.RelativePath, dto.SizeInBytes, dto.ShardMaxSizeInBytes);
    }
}