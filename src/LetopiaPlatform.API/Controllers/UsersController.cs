using LetopiaPlatform.API.AppMetaData;
using LetopiaPlatform.API.Extensions;
using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.DTOs.Email;
using LetopiaPlatform.Core.DTOs.User;
using LetopiaPlatform.Core.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Text.Json;

namespace LetopiaPlatform.API.Controllers;

[Authorize]
public class UsersController : BaseController
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;


    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    // ── Profile ───────────────────────────────────────────────────────────

    /// <summary>
    /// Get current user profile.
    /// </summary>
    [HttpGet(Router.Users.Me)]
    public async Task<IActionResult> GetCurrentUser()
    {
        HttpContext.AddBusinessContext("action", "get_profile");
        var result = await _userService.GetProfileAsync(GetUserId(), HttpContext.RequestAborted);
        return HandleResult(result);
    }
    /// <summary>
    /// Get public profile for a specific user.
    /// Returns limited data based on privacy settings.
    /// </summary>
    [AllowAnonymous]
    [HttpGet(Router.Users.GetById)]
    public async Task<IActionResult> GetUserById(Guid id, CancellationToken cancellationToken)
    {
        HttpContext.AddBusinessContext("action", "get_public_profile");
        HttpContext.AddBusinessContext("target_user_id", id);

        Guid? currentUserId = null;

        
        if (User.Identity?.IsAuthenticated == true)
        {
            currentUserId = GetUserId();
        }

        var result = await _userService.GetPublicProfileAsync(
            id,
            currentUserId,
            cancellationToken);

        return HandleResult(result);
    }
    /// <summary>
    /// Update current user profile.
    /// Email is not updatable here — use the email change flow.
    /// </summary>
    [HttpPut(Router.Users.Update)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        HttpContext.AddBusinessContext("action", "update_profile");





        var result = await _userService.UpdateProfileAsync(
            GetUserId(),
            request,
            HttpContext.RequestAborted);

        return HandleResult(result);
    }

    // ── Avatar ────────────────────────────────────────────────────────────

    /// <summary>
    /// Upload or replace user avatar.
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
    /// Remove user avatar.
    /// </summary>
    [HttpDelete(Router.Users.Avatar)]
    public async Task<IActionResult> DeleteAvatar(CancellationToken cancellationToken)
    {
        HttpContext.AddBusinessContext("action", "delete_avatar");
        var result = await _userService.DeleteAvatarAsync(GetUserId(), cancellationToken);
        return HandleResult(result);
    }

    // ── Email change ──────────────────────────────────────────────────────

    /// <summary>
    /// Request an email address change.
    /// </summary>
    [HttpPost(Router.Users.EmailChangeRequest)]

    public async Task<IActionResult> RequestEmailChange(
        [FromBody] EmailChangeRequest request, CancellationToken cancellationToken)
    {
        HttpContext.AddBusinessContext("action", "email_change_request");
        var result = await _userService.RequestEmailChangeAsync(GetUserId(), request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Confirm an email change using the token from the confirmation link.
    /// </summary>
    [AllowAnonymous]
    [HttpPost(Router.Users.EmailChangeConfirm)]
    public async Task<IActionResult> ConfirmEmailChange(
        [FromBody] EmailConfirmRequest request, CancellationToken cancellationToken)
    {
        HttpContext.AddBusinessContext("action", "email_change_confirm");
        var result = await _userService.ConfirmEmailChangeAsync(request, cancellationToken);
        return HandleResult(result);
    }

    // ── Preferences ───────────────────────────────────────────────────────

    /// <summary>
    /// Update notification preferences and privacy settings.
    /// </summary>
    [HttpPut(Router.Users.Preferences)]
    public async Task<IActionResult> UpdatePreferences(
        [FromBody] UpdatePreferencesRequest request, CancellationToken cancellationToken)
    {
        HttpContext.AddBusinessContext("action", "update_preferences");
        var result = await _userService.UpdatePreferencesAsync(GetUserId(), request, cancellationToken);
        return HandleResult(result);
    }

    // ── Account ───────────────────────────────────────────────────────────

    /// <summary>
    /// Permanently delete current user account (GDPR soft-delete).
    /// </summary>
    [HttpDelete(Router.Users.Me)]
    
    public async Task<IActionResult> DeleteAccount(CancellationToken cancellationToken)
    {
        HttpContext.AddBusinessContext("action", "delete_account");
        var result = await _userService.DeleteAccountAsync(GetUserId(), cancellationToken);
        return HandleResult(result);
    }
}
