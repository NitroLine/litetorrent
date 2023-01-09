using LiteTorrent.Backend.Dto;
using Microsoft.AspNetCore.Mvc;

namespace LiteTorrent.Backend;

[ApiController]
[Route("commands")]
[Consumes("application/json")]
[Produces("application/json")]
public class CommandController : ControllerBase
{
    [HttpPost]
    public Task<ActionResult> StartDownloading(
        [FromBody] DtoStartDownloadingInfo dtoStartDownloadingInfo)
    {
        return Task.FromResult<ActionResult>(Ok("OK"));
    }
    
    [HttpGet("sharedFiles")]
    public Task<ActionResult<DtoSharedFile[]>> GetSharedFiles()
    {
        return Task.FromResult((ActionResult<DtoSharedFile[]>)new []{new DtoSharedFile("hash", "rel", 0.1)});
    }
}