using System.Runtime.CompilerServices;
using LiteTorrent.Domain.Services.Common.Serialization;
using MessagePack;
using MessagePack.Resolvers;
using SimpleBase;

namespace LiteTorrent.Domain.Services.LocalStorage.Common;

public static class LocalStorageHelper
{
    public static readonly MessagePackSerializerOptions SerializerOptions = new(
        CompositeResolver.Create(
            StandardResolver.Instance,
            HashResolver.Instance,
            DnsEndpointResolver.Instance));

    private static readonly FileStreamOptions DefaultFileStreamOptionsToRead = new();

    private static readonly FileStreamOptions DefaultFileStreamOptionsToWrite = new()
    {
        Mode = FileMode.Open,
        Access = FileAccess.Write, 
        Share = FileShare.Write
    };

    public static FileStream GetFileStreamToWrite(string path)
    {
        Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
        Console.WriteLine(path);
        for (var i = 0; i < 5; i++)
        {
            try
            {
                var fileInfo = new FileInfo(path);
                if (!fileInfo.Exists)
                    File.Create(path);

                return new FileStream(path, DefaultFileStreamOptionsToWrite);
            }
            catch (IOException e)
            {
                if (i == 4)
                    throw;
            }
        }

        throw new Exception();
    }

    public static FileStream GetFileStreamToRead(string path)
    {
        for (var i = 0; i < 5; i++)
        {
            try
            {
                var fileInfo = new FileInfo(path);
                if (!fileInfo.Exists)
                    File.Create(path);

                return new FileStream(path, DefaultFileStreamOptionsToRead);
            }
            catch (IOException)
            {
                if (i == 4)
                    throw;
            }
        }

        throw new Exception();
    }

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
}