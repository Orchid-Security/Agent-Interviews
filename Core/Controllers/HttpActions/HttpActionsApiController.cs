using System.Diagnostics;
using System.IO.Compression;
using Microsoft.AspNetCore.Mvc;

namespace Core.Controllers.HttpActions;

[ApiController]
[Route("actions")]
public class HttpActionsApiController : ControllerBase
{
    [HttpGet]
    [Route("processes")]
    public IActionResult GetAllProcesses()
    {
        var processes = Process.GetProcesses()
            .Select(p => new { p.Id, p.ProcessName })
            .ToList();
        return Ok(processes);
    }

    [HttpGet]
    [Route("zip")]
    public IActionResult ZipAndReturnDirectory([FromQuery] string sourceDir)
    {
		var zipFileName = $"{Path.GetFileName(sourceDir)}.zip";

        var zipFilePath = Path.Combine(Path.GetTempPath(), zipFileName);

        ZipFile.CreateFromDirectory(sourceDir, zipFilePath, CompressionLevel.Optimal, false);

        var zipFileStream = new FileStream(zipFilePath, FileMode.Open, FileAccess.Read);
        return File(zipFileStream, "application/zip", zipFileName);
    }

	[HttpGet]
    [Route("list-files")]
    public IActionResult ListFilesInDirectory([FromQuery] string path, [FromQuery] int maxDepth)
    {
        var files = HttpActionsUtils.ListFiles(path, maxDepth).ToList();
        return Ok(files);
    }
}
