using LetopiaPlatform.API.AppMetaData;
using LetopiaPlatform.API.DTOs.User;
using LetopiaPlatform.API.Extensions;
using LetopiaPlatform.Core.Common;
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

    public UsersController(IUserService userService)
    {
        _userService = userService;
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

    // ── Avatar ────────────────────────────────────────────────────────────

    /// <summary>
    /// Upload or replace user avatar
    /// </summary>
    [HttpPut(Router.Users.Avatar)]
    [EnableRateLimiting(RateLimitingExtensions.FileUploadPolicy)]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> UpdateAvatar(IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return HandleResult(Result<string>.Failure("No file provided"));

        HttpContext.AddBusinessContext("action", "update_avatar");
        HttpContext.AddBusinessContext("file_size_bytes", file.Length);

        var result = await _userService.UpdateAvatarAsync(GetUserId(), file, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Remove user avatar
    /// </summary>
    [HttpDelete(Router.Users.Avatar)]
    public async Task<IActionResult> DeleteAvatar(CancellationToken cancellationToken)
    {
        HttpContext.AddBusinessContext("action", "delete_avatar");

        var result = await _userService.DeleteAvatarAsync(GetUserId(), cancellationToken);
        return HandleResult(result);
    }
}
