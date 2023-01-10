using LiteTorrent.Domain.Services.LocalStorage.Common;
using MessagePack;

namespace LiteTorrent.Domain.Services.StateRepository;

public class StateRepository
{
    public async Task Write(State state, CancellationToken cancellationToken)
    {
        await using var file = new FileStream("state", FileMode.OpenOrCreate, FileAccess.Write);
       
        await MessagePackSerializer.SerializeAsync(
            file,
            state,
            LocalStorageHelper.SerializerOptions, 
            cancellationToken);
    }

    public async Task<State?> Read(CancellationToken cancellationToken)
    {
        if (!File.Exists("state"))
            return null;
        
        await using var file = new FileStream("state", FileMode.Open, FileAccess.Read);
        
        return await MessagePackSerializer.DeserializeAsync<State>(
            file,
            LocalStorageHelper.SerializerOptions,
            cancellationToken);
    }
}