using LiteTorrent.Domain.Services.PieceExchange;
using Microsoft.Extensions.Hosting;

namespace LiteTorrent.Backend;

public class LiteTorrentService : BackgroundService
{
    private readonly PieceExchanger exchanger;

    public LiteTorrentService(PieceExchanger exchanger)
    {
        this.exchanger = exchanger;
    }
    
    protected override Task ExecuteAsync(CancellationToken cancellationToken)
    {
        return exchanger.StartDistributing(cancellationToken);
    }
}