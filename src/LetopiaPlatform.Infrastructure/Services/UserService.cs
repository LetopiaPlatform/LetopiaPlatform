using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.DTOs.User;
using LetopiaPlatform.Core.Entities.Identity;
using LetopiaPlatform.Core.Interfaces;
using LetopiaPlatform.Core.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace LetopiaPlatform.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly IGenericRepository<User> _userRepository;
    private readonly IFileStorageService _fileService;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IGenericRepository<User> userRepository,
        IFileStorageService fileService,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _fileService = fileService;
        _logger = logger;
    }

    public async Task<Result<UserProfileResponse>> GetProfileAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
        {
            _logger.LogWarning("Profile not found for user {UserId}", userId);
            return Result<UserProfileResponse>.Failure("User not found", 404);
        }

        return Result<UserProfileResponse>.Success(MapToResponse(user));
    }

    public async Task<Result<UserProfileResponse>> UpdateProfileAsync(
        Guid userId, UpdateProfileRequest request, IFormFile? avatar, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
        {
            _logger.LogWarning("Cannot update profile — user {UserId} not found", userId);
            return Result<UserProfileResponse>.Failure("User not found", 404);
        }

        // Apply non-null fields (partial update — null means "don't change", empty string means "clear")
        if (request.FullName is not null) user.FullName = request.FullName;
        if (request.Bio is not null) user.Bio = request.Bio;
        if (request.PhoneNumber is not null) user.PhoneNumber = request.PhoneNumber;
        if (request.DateOfBirth.HasValue) user.DateOfBirth = request.DateOfBirth.Value;

        // TODO: Email change should require re-verification (send confirmation link).
        // For now, allow direct update but log it for audit.
        if (request.Email is not null)
        {
            _logger.LogInformation("User {UserId} changing email from {OldEmail} to {NewEmail}",
                userId, user.Email, request.Email);
            user.Email = request.Email;
            user.UserName = request.Email; // Keep in sync with Identity
        }

        if (avatar is not null)
        {
            var uploadResult = await _fileService.ReplaceAsync(avatar, "avatars", user.AvatarUrl);
            if (!uploadResult.IsSuccess)
            {
                _logger.LogError("Avatar upload failed for user {UserId}: {Errors}",
                    userId, string.Join(", ", uploadResult.Errors));
                return Result<UserProfileResponse>.Failure(uploadResult.Errors);
            }

            user.AvatarUrl = uploadResult.Value!;
        }

        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        _logger.LogInformation("Profile updated for user {UserId}", userId);
        return Result<UserProfileResponse>.Success(MapToResponse(user));
    }

    private static UserProfileResponse MapToResponse(User user) => new(
        Id: user.Id,
        FullName: user.FullName ?? string.Empty,
        Email: user.Email ?? string.Empty,
        Bio: user.Bio,
        PhoneNumber: user.PhoneNumber,
        DateOfBirth: user.DateOfBirth,
        AvatarUrl: user.AvatarUrl);
}
