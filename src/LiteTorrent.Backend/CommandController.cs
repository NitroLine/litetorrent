using System.Net.Mime;
using LiteTorrent.Backend.Dto;
using LiteTorrent.Core;
using LiteTorrent.Domain;
using LiteTorrent.Domain.Services.Commands;
using LiteTorrent.Domain.Services.PieceExchange;
using MessagePipe;
using Microsoft.AspNetCore.Mvc;
using SimpleBase;

namespace LiteTorrent.Backend;

[Route("commands")]
[Consumes(MediaTypeNames.Application.Json)]
[Produces(MediaTypeNames.Application.Json)]
public class CommandController : ControllerBase
{
    private readonly PieceExchanger pieceExchanger;
    private readonly IAsyncRequestHandler<CreateSharedFileCommand, Result<Unit>> createHandler;
    private readonly IAsyncRequestHandler<GetSharedFilesCommand, Result<SharedFile[]>> getSharedFilesHandler;
    private readonly IAsyncRequestHandler<AddSharedFileCommand, Result<Unit>> addHandler;

    public CommandController(
        PieceExchanger pieceExchanger,
        IAsyncRequestHandler<CreateSharedFileCommand, Result<Unit>> createHandler,
        IAsyncRequestHandler<GetSharedFilesCommand, Result<SharedFile[]>> getSharedFilesHandler,
        IAsyncRequestHandler<AddSharedFileCommand, Result<Unit>> addHandler)
    {
        this.pieceExchanger = pieceExchanger;
        this.createHandler = createHandler;
        this.getSharedFilesHandler = getSharedFilesHandler;
        this.addHandler = addHandler;
    }
    
    [HttpPost("create")]
    public async Task<ActionResult> CreateTorrentFile(
        [FromBody] DtoCreateInfo createInfo,
        CancellationToken cancellationToken)
    {
        var command = new CreateSharedFileCommand(
            createInfo.RelativePath,
            createInfo.ShardMaxSizeInBytes,
            createInfo.OutputFileAbsolutePath);

        var result = await createHandler.InvokeAsync(command, cancellationToken);
        if (result.TryGetError(out _, out var error))
            throw new Exception(error.Message);

        return NoContent();
    }

    [HttpPost("add")]
    public async Task<ActionResult> AddTorrentFile(
        [FromBody] DtoTorrentFile torrentFile, 
        CancellationToken cancellationToken)
    {
        var command = new AddSharedFileCommand(torrentFile.FileFullName);
        var result = await addHandler.InvokeAsync(command, cancellationToken);
        if (result.TryGetError(out _, out var error))
            throw new Exception(error.Message);

        return NoContent();
    }
    
    [HttpGet("show")]
    public async Task<ActionResult<DtoSharedFile[]>> GetSharedFiles(CancellationToken cancellationToken)
    {
        var result = await getSharedFilesHandler.InvokeAsync(new GetSharedFilesCommand(), cancellationToken);
        if (result.TryGetError(out var sharedFiles, out var error))
            throw new Exception(error.Message);

        return sharedFiles
            .Select(sharedFile => new DtoSharedFile(
                Base32.Rfc4648.Encode(sharedFile.Hash.Data.Span), 
                sharedFile.RelativePath,
                sharedFile.SizeInBytes,
                0,
                0,
                false, 
                false))
            .ToArray();
    }
}