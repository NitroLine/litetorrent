using LiteTorrent.Domain.Services.ShardExchange;
using Microsoft.Extensions.Hosting;

namespace LiteTorrent.Client.Cli;

public class LiteTorrentService : BackgroundService
{
    private readonly ShardExchanger exchanger;

    public LiteTorrentService(ShardExchanger exchanger)
    {
        this.exchanger = exchanger;
    }
    
    protected override Task ExecuteAsync(CancellationToken cancellationToken)
    {
        return Task.WhenAll(
            exchanger.StartDistributing(cancellationToken),
            exchanger.StartDownloading(cancellationToken));
    }
}