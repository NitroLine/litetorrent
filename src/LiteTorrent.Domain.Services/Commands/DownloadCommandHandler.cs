using System.Net;
using LiteTorrent.Core;
using MessagePipe;

namespace LiteTorrent.Domain.Services.Commands;

public record DownloadCommand(DnsEndPoint[] Hosts, Hash SharedFileHash);

public class DownloadCommandHandler : IAsyncRequestHandler<DownloadCommand, Result<Unit>>
{
    public ValueTask<Result<Unit>> InvokeAsync(
        DownloadCommand request,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}