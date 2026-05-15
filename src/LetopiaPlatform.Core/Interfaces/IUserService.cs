using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.DTOs.Email;
using LetopiaPlatform.Core.DTOs.User;
using Microsoft.AspNetCore.Http;

namespace LetopiaPlatform.Core.Services.Interfaces;

/// <summary>
/// Manages user profile retrieval, updates, email changes, preferences, and account deletion.
/// </summary>
public interface IUserService
{
    // ── Profile ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Retrieves a user's profile by their unique identifier.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>A result containing the user profile on success.</returns>
    Task<Result<UserProfileResponse>> GetProfileAsync(Guid userId, CancellationToken ct = default);
    /// <summary>
    /// Retrieves a public view of a user's profile with respect to privacy settings.
    /// Returns limited data when accessed by other users.
    /// </summary>
    /// <param name="targetUserId">The user whose profile is being viewed.</param>
    /// <param name="currentUserId">The currently authenticated user (optional).</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>A result containing the public profile data.</returns>
    Task<Result<PublicUserProfileResponse>> GetPublicProfileAsync(
        Guid targetUserId,
        Guid? currentUserId = null,
        CancellationToken ct = default);
    /// <summary>
    /// Updates a user's profile with the provided data and optional avatar image.
    /// Only non-null fields are applied — null means "leave unchanged".
    /// </summary>
    /// <param name="userId">The unique identifier of the user to update.</param>
    /// <param name="request">The profile fields to update.</param>

    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>A result containing the updated user profile on success.</returns>
    Task<Result<UserProfileResponse>> UpdateProfileAsync(
        Guid userId, UpdateProfileRequest request, CancellationToken ct = default);

    /// <summary>
    /// Replaces the user's avatar with the provided file.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="avatar">The new avatar image file to upload.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>A result containing the updated user profile on success.</returns>
    Task<Result<UserProfileResponse>> UpdateAvatarAsync(
        Guid userId, IFormFile avatar, CancellationToken ct = default);

    /// <summary>
    /// Removes the user's avatar and clears the avatar URL from their profile.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>A result containing the updated user profile on success.</returns>
    Task<Result<UserProfileResponse>> DeleteAvatarAsync(Guid userId, CancellationToken ct = default);

    // ── Email change ─────────────────────────────────────────────────────────

    /// <summary>
    /// Initiates an email change request for the user.
    /// Sends a confirmation link to the new address and a security notice to the current address.
    /// Any previously active pending request for this user is invalidated.
    /// </summary>
    /// <param name="userId">The unique identifier of the user requesting the change.</param>
    /// <param name="request">Contains the new email address.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>A result indicating whether the request was successfully initiated.</returns>
    Task<Result> RequestEmailChangeAsync(
        Guid userId, EmailChangeRequest request, CancellationToken ct = default);

    /// <summary>
    /// Confirms an email change using the token from the confirmation link.
    /// Applies the new email, sets EmailVerified to true, and rotates the security stamp
    /// to invalidate all existing sessions.
    /// </summary>
    /// <param name="request">Contains the userId and raw token from the confirmation link.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>A result indicating whether the email was successfully changed.</returns>
    Task<Result> ConfirmEmailChangeAsync(
        EmailConfirmRequest request, CancellationToken ct = default);

    // ── Preferences ──────────────────────────────────────────────────────────

    /// <summary>
    /// Updates the user's notification preferences, social links, and privacy settings.
    /// Only non-null fields are applied — null means "leave unchanged".
    /// Kept separate from profile update to allow independent caching.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="request">The preference fields to update.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>A result containing the updated user profile on success.</returns>
    Task<Result<UserProfileResponse>> UpdatePreferencesAsync(
        Guid userId, UpdatePreferencesRequest request, CancellationToken ct = default);

    // ── Account ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Soft-deletes the user's account by anonymizing all personally identifiable information.
    /// The row is retained for referential integrity with projects and memberships.
    /// Rotates the security stamp to invalidate all active sessions immediately.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to delete.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>A result indicating whether the account was successfully anonymized.</returns>
    Task<Result> DeleteAccountAsync(Guid userId, CancellationToken ct = default);
}
