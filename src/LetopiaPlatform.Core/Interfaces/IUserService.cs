using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.DTOs.User;
using Microsoft.AspNetCore.Http;

namespace LetopiaPlatform.Core.Services.Interfaces;

/// <summary>
/// Manages user profile retrieval and updates.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Retrieves a user's profile by their unique identifier.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>A result containing the user profile on success.</returns>
    Task<Result<UserProfileResponse>> GetProfileAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Updates a user's profile with the provided data and optional avatar image.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to update.</param>
    /// <param name="request">The profile fields to update.</param>
    /// <param name="avatar">An optional avatar image file to upload.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>A result containing the updated user profile on success.</returns>
    Task<Result<UserProfileResponse>> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, IFormFile? avatar, CancellationToken ct = default);
}
