using System.Runtime.CompilerServices;
using LiteTorrent.Core;
using LiteTorrent.Domain.Services.Common.Serialization;
using MessagePack;
using MessagePack.Resolvers;
using SimpleBase;

namespace LiteTorrent.Domain.Services.LocalStorage.Common;

public static class LocalStorageHelper
{
    public static readonly FilePool FilePool = new();
    
    public static readonly MessagePackSerializerOptions SerializerOptions = new(
        CompositeResolver.Create(
            StandardResolver.Instance,
            HashResolver.Instance,
            DnsEndpointResolver.Instance));

    private static readonly FileStreamOptions DefaultFileStreamOptionsToRead = new()
    {
        Mode = FileMode.Open,
        Access = FileAccess.Read
    };

    private static readonly FileStreamOptions DefaultFileStreamOptionsToWrite = new()
    {
        Mode = FileMode.OpenOrCreate,
        Access = FileAccess.Write
    };

    // public static FileStream GetFileStreamToWrite(string path)
    // {
    //     return new FileStream(path, DefaultFileStreamOptionsToWrite);
    // }
    //
    // public static FileStream GetFileStreamToRead(string path)
    // {
    //     return new FileStream(path, DefaultFileStreamOptionsToRead);
    // }

    public static string GetFilePath(string baseDir, Hash hash)
    {
        return Path.Join(baseDir, Base32.Rfc4648.Encode(hash.Data.Span));
    }

    public static async IAsyncEnumerable<Piece> SplitData(
        Stream dataStream, 
        uint maxShardSizeInBytes, 
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var buffer = new byte[maxShardSizeInBytes];
        var count = await dataStream.ReadAsync(buffer, cancellationToken);
        var index = 0ul;
        while (count != 0)
        {
            yield return new Piece(index, new ReadOnlyMemory<byte>(buffer, 0, count));
            
            count = await dataStream.ReadAsync(buffer, cancellationToken);
            index++;
        }
    }

    public static Task<FileLock> GetToRead(this FilePool pool, string fullFileName)
    {
        return pool.GetAsync(fullFileName, DefaultFileStreamOptionsToRead);
    }
    
    public static Task<FileLock> GetToWrite(this FilePool pool, string fullFileName)
    {
        return pool.GetAsync(fullFileName, DefaultFileStreamOptionsToWrite);
    }
}