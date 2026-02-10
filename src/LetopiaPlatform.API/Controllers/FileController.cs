
using LetopiaPlatform.API.AppMetaData;
using LetopiaPlatform.API.DTOs.File;
using LetopiaPlatform.Core.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LetopiaPlatform.API.Controllers;

[Route("api/[controller]")]
[Authorize]
public class FileController : BaseController
{
    private readonly IFileStorageService _fileService;

    public FileController(IFileStorageService fileService)
    {
        _fileService = fileService;
    }

    /// <summary>
    /// Upload a single file
    /// </summary>
    [HttpPost(Router.File.Upload)]
    public async Task<IActionResult> Upload(IFormFile file, [FromQuery] string directory = "uploads")
    {
        var result = await _fileService.UploadAsync(file, directory);
        return HandleResult(result);
    }

    /// <summary>
    /// Upload multiple files
    /// </summary>
    [HttpPost(Router.File.UploadMultiple)]
    public async Task<IActionResult> UploadMultiple(List<IFormFile> files, [FromQuery] string directory = "uploads")
    {
        var result = await _fileService.UploadManyAsync(files, directory);
        return HandleResult(result);
    }

    /// <summary>
    /// Replace an existing file with a new one
    /// </summary>
    [HttpPut(Router.File.Replace)]
    public async Task<IActionResult> Replace([FromForm] ReplaceFileDto dto)
    {
        var result = await _fileService.ReplaceAsync(dto.NewFile, dto.Directory, dto.OldFileUrl);
        return HandleResult(result);
    }

    /// <summary>
    /// Delete a file by URL
    /// </summary>
    [HttpDelete(Router.File.Delete)]
    public async Task<IActionResult> Delete([FromQuery] string fileUrl)
    {
        var result = await _fileService.DeleteAsync(fileUrl);
        return HandleResult(result);
    }
}
