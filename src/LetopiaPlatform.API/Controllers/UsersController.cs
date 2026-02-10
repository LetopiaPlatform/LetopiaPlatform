using LetopiaPlatform.API.AppMetaData;
using LetopiaPlatform.API.DTOs.File;
using LetopiaPlatform.API.DTOs.User;
using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.Entities.Identity;
using LetopiaPlatform.Core.Interfaces;
using LetopiaPlatform.Core.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LetopiaPlatform.API.Controllers;

[Authorize]
public class UsersController : BaseController
{
    private readonly IGenericRepository<User> _userRepository;
    private readonly IFileStorageService _fileService;

    public UsersController(IGenericRepository<User> userRepository, IFileStorageService fileService)
    {
        _userRepository = userRepository;
        _fileService = fileService;
    }

    // ── Profile ───────────────────────────────────────────────────────────

    /// <summary>
    /// Get current user profile
    /// </summary>
    [HttpGet(Router.Users.Me)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = GetUserId();
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return HandleResult(Result.Failure("User not found"));

        return HandleResult(Result<UserProfileResponseDto>.Success(MapToDto(user)));
    }

    /// <summary>
    /// Update current user profile
    /// </summary>
    [HttpPut(Router.Users.Update)]
    public async Task<IActionResult> UpdateProfile([FromForm] UpdateUserProfileDto dto)
    {
        var userId = GetUserId();
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return HandleResult(Result.Failure("User not found"));

        if (!string.IsNullOrEmpty(dto.FullName)) user.FullName = dto.FullName;
        if (!string.IsNullOrEmpty(dto.Email)) user.Email = dto.Email;
        if (!string.IsNullOrEmpty(dto.Bio)) user.Bio = dto.Bio;
        if (!string.IsNullOrEmpty(dto.PhoneNumber)) user.PhoneNumber = dto.PhoneNumber;
        if (dto.DateOfBirth.HasValue) user.DateOfBirth = dto.DateOfBirth.Value;

        if (dto.AvatarUrl != null)
        {
            var uploadResult = await _fileService.ReplaceAsync(dto.AvatarUrl, "avatars", user.AvatarUrl);
            if (!uploadResult.IsSuccess) return HandleResult(uploadResult);

            user.AvatarUrl = uploadResult.Value!;
        }

        await _userRepository.UpdateAsync(user);

        return HandleResult(Result<UserProfileResponseDto>.Success(MapToDto(user)));
    }

    // ── Files ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Upload a single file
    /// </summary>
    [HttpPost(Router.Users.UploadFile)]
    public async Task<IActionResult> UploadFile(IFormFile file, [FromQuery] string directory = "uploads")
    {
        var result = await _fileService.UploadAsync(file, directory);
        return HandleResult(result);
    }

    /// <summary>
    /// Upload multiple files
    /// </summary>
    [HttpPost(Router.Users.UploadMultipleFiles)]
    public async Task<IActionResult> UploadMultipleFiles(List<IFormFile> files, [FromQuery] string directory = "uploads")
    {
        var result = await _fileService.UploadManyAsync(files, directory);
        return HandleResult(result);
    }

    /// <summary>
    /// Replace an existing file with a new one
    /// </summary>
    [HttpPut(Router.Users.ReplaceFile)]
    public async Task<IActionResult> ReplaceFile([FromForm] ReplaceFileDto dto)
    {
        var result = await _fileService.ReplaceAsync(dto.NewFile, dto.Directory, dto.OldFileUrl);
        return HandleResult(result);
    }

    /// <summary>
    /// Delete a file by URL
    /// </summary>
    [HttpDelete(Router.Users.DeleteFile)]
    public async Task<IActionResult> DeleteFile([FromQuery] string fileUrl)
    {
        var result = await _fileService.DeleteAsync(fileUrl);
        return HandleResult(result);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private static UserProfileResponseDto MapToDto(User user) => new()
    {
        Id = user.Id,
        FullName = user.FullName ?? string.Empty,
        Email = user.Email ?? string.Empty,
        Bio = user.Bio,
        PhoneNumber = user.PhoneNumber,
        DateOfBirth = user.DateOfBirth,
        AvatarUrl = user.AvatarUrl
    };
}
