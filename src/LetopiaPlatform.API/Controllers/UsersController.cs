using LetopiaPlatform.API.AppMetaData;
using LetopiaPlatform.API.DTOs.User;
using LetopiaPlatform.API.Extensions;
using LetopiaPlatform.Core.DTOs.User;
using LetopiaPlatform.Core.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace LetopiaPlatform.API.Controllers;

[Authorize]
public class UsersController : BaseController
{
    private readonly IUserService _userService;
    private readonly IFileStorageService _fileService;

    public UsersController(IUserService userService, IFileStorageService fileService)
    {
        _userService = userService;
        _fileService = fileService;
    }

    // ── Profile ───────────────────────────────────────────────────────────

    /// <summary>
    /// Get current user profile
    /// </summary>
    [HttpGet(Router.Users.Me)]
    public async Task<IActionResult> GetCurrentUser()
    {
        HttpContext.AddBusinessContext("action", "get_profile");

        var result = await _userService.GetProfileAsync(GetUserId(), HttpContext.RequestAborted);
        return HandleResult(result);
    }

    /// <summary>
    /// Update current user profile
    /// </summary>
    [HttpPut(Router.Users.Update)]
    public async Task<IActionResult> UpdateProfile([FromForm] UpdateUserProfileDto dto)
    {
        HttpContext.AddBusinessContext("action", "update_profile");
        HttpContext.AddBusinessContext("has_avatar", dto.AvatarUrl is not null);

        var request = new UpdateProfileRequest(
            dto.FullName,
            dto.Email,
            dto.Bio,
            dto.PhoneNumber);

        var result = await _userService.UpdateProfileAsync(GetUserId(), request, dto.AvatarUrl, HttpContext.RequestAborted);
        return HandleResult(result);
    }

    // ── Files ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Upload a single file
    /// </summary>
    [HttpPost(Router.Users.UploadFile)]
    [EnableRateLimiting(RateLimitingExtensions.FileUploadPolicy)]
    public async Task<IActionResult> UploadFile(IFormFile file, [FromQuery] string directory = "uploads")
    {
        HttpContext.AddBusinessContext("action", "upload_file");
        HttpContext.AddBusinessContext("directory", directory);
        HttpContext.AddBusinessContext("file_name", file.FileName);
        HttpContext.AddBusinessContext("file_size_bytes", file.Length);

        var result = await _fileService.UploadAsync(file, directory);
        return HandleResult(result);
    }

    /// <summary>
    /// Delete a file by URL
    /// </summary>
    [HttpDelete(Router.Users.DeleteFile)]
    public async Task<IActionResult> DeleteFile([FromQuery] string fileUrl)
    {
        HttpContext.AddBusinessContext("action", "delete_file");
        HttpContext.AddBusinessContext("file_url", fileUrl);

        var result = await _fileService.DeleteAsync(fileUrl);
        return HandleResult(result);
    }
}
