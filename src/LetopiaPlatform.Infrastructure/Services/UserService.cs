using System.Security.Cryptography;
using System.Text;
using LetopiaPlatform.Core.AppSettings;
using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.DTOs.Email;
using LetopiaPlatform.Core.DTOs.User;
using LetopiaPlatform.Core.Entities.Identity;
using LetopiaPlatform.Core.Enums;
using LetopiaPlatform.Core.Interfaces;
using LetopiaPlatform.Core.Services.Interfaces;
using LetopiaPlatform.Infrastructure.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LetopiaPlatform.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly IGenericRepository<User> _userRepository;
    private readonly IGenericRepository<PendingEmailChange> _pendingEmailRepository;
    private readonly IFileStorageService _fileService;
    private readonly ILogger<UserService> _logger;
    private readonly IEmailService _emailService;
    private readonly string _frontendBaseUrl;
    private readonly string _assetsBaseUrl;

    public UserService(
        IGenericRepository<User> userRepository,
        IGenericRepository<PendingEmailChange> pendingEmailRepository,
        IFileStorageService fileService,
          IEmailService emailService,
           IOptions<SmtpSettings> smtpSettings,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _pendingEmailRepository = pendingEmailRepository;
        _fileService = fileService;
        _emailService = emailService;
        _logger = logger;
        _frontendBaseUrl = smtpSettings.Value.FrontendBaseUrl.TrimEnd('/');
        _assetsBaseUrl = smtpSettings.Value.EmailAssetsBaseUrl.TrimEnd('/');
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
    public async Task<Result<PublicUserProfileResponse>> GetPublicProfileAsync(
    Guid targetUserId,
    Guid? currentUserId = null,
    CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(targetUserId);

        if (user is null)
        {
            _logger.LogWarning("User {UserId} not found", targetUserId);
            return Result<PublicUserProfileResponse>.Failure("User not found", 404);
        }

        
        if (currentUserId == targetUserId)
        {
            return Result<PublicUserProfileResponse>.Success(MapToPublicResponse(user));
        }

        // 👇 Privacy check
        if (user.PrivacySettings?.ProfileVisibility == ProfileVisibility.Private)
        {
            return Result<PublicUserProfileResponse>.Failure("Profile is private", 403);
        }

        return Result<PublicUserProfileResponse>.Success(MapToPublicResponse(user));
    }

    public async Task<Result<UserProfileResponse>> UpdateProfileAsync(
     Guid userId,
     UpdateProfileRequest request,
     CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
        {
            _logger.LogWarning("Cannot update profile — user {UserId} not found", userId);
            return Result<UserProfileResponse>.Failure("User not found", 404);
        }

        // ── Basic profile ────────────────────────────────────────────────
        if (request.FullName is not null) user.FullName = request.FullName;
        if (request.Bio is not null) user.Bio = request.Bio;
        if (request.PhoneNumber is not null) user.PhoneNumber = request.PhoneNumber;
        if (request.Location is not null) user.Location = request.Location;
        if (request.Interests is not null) user.Interests = request.Interests;
        if (request.Skills is not null) user.Skills = request.Skills;

        if (request.SocialLinks is not null)
        {
            user.SocialLinks ??= [];
            user.SocialLinks.Clear();
            user.SocialLinks.AddRange(
                request.SocialLinks
                    .DistinctBy(s => s.Provider.ToLowerInvariant())
                    .Select(s => new SocialLink(s.Provider, s.Url))
            );
        }



        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);

        _logger.LogInformation("Profile updated for user {UserId}", userId);

        return Result<UserProfileResponse>.Success(MapToResponse(user));
    }
    public async Task<Result<UserProfileResponse>> UpdatePreferencesAsync(
        Guid userId, UpdatePreferencesRequest request, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            return Result<UserProfileResponse>.Failure("User not found", 404);

        if (request.NotificationPreferences is not null)
            user.NotificationPreferences = request.NotificationPreferences;



        if (request.PrivacySettings is not null)
            user.PrivacySettings = request.PrivacySettings;

        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        _logger.LogInformation("Preferences updated for user {UserId}", userId);
        return Result<UserProfileResponse>.Success(MapToResponse(user));
    }
    public async Task<Result> RequestEmailChangeAsync(
        Guid userId, EmailChangeRequest request, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            return Result.Failure("User not found", 404);

        // Block if already in use
        var normalizedNew = request.NewEmail.ToUpperInvariant();
        var taken = await _userRepository.FindAsync(u => u.NormalizedEmail == normalizedNew);
        if (taken.Any())
            return Result.Failure("Email already in use", 409);

        // Invalidate any active pending request
        var previous = await _pendingEmailRepository
            .FindAsync(p => p.UserId == userId && !p.IsUsed && p.ExpiresAt > DateTime.UtcNow);

        foreach (var pending in previous)
        {
            pending.IsUsed = true;
            await _pendingEmailRepository.UpdateAsync(pending);
        }

        // Generate URL-safe token
        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
                        .Replace("+", "-").Replace("/", "_").TrimEnd('=');

        var newPending = new PendingEmailChange
        {
            UserId = userId,
            NewEmail = request.NewEmail,
            Token = HashToken(rawToken),
            ExpiresAt = DateTime.UtcNow.AddHours(24),
        };
        await _pendingEmailRepository.AddAsync(newPending);

        var confirmUrl = $"{_frontendBaseUrl}/confirm-email?token={rawToken}&userId={userId}";

        // 1 — confirmation link to NEW email
        await _emailService.SendEmailChangeConfirmationAsync(
            request.NewEmail, user.FullName ?? user.UserName!, confirmUrl, ct);

        // 2 — security notice to OLD email
        await _emailService.SendEmailChangeNotificationAsync(
            user.Email!, user.FullName ?? user.UserName!, request.NewEmail, ct);
        var safeNewEmailForLog = (request.NewEmail ?? string.Empty)
    .Replace("\r", string.Empty)
    .Replace("\n", string.Empty);
        _logger.LogInformation("Email change requested for user {UserId} → {NewEmail}", userId, safeNewEmailForLog);
      
        return Result.Success();
    }

    public async Task<Result> ConfirmEmailChangeAsync(
        EmailConfirmRequest request, CancellationToken ct = default)
    {
        var hashed = HashToken(request.Token);

        var matches = await _pendingEmailRepository
            .FindAsync(p => p.UserId == request.UserId && p.Token == hashed && !p.IsUsed);

        var pending = matches.FirstOrDefault();

        if (pending is null)
            return Result.Failure("Invalid or expired token", 400);

        if (pending.ExpiresAt < DateTime.UtcNow)
        {
            pending.IsUsed = true;
            await _pendingEmailRepository.UpdateAsync(pending);
            return Result.Failure("Token has expired", 400);
        }

        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user is null)
            return Result.Failure("User not found", 404);

        user.Email = pending.NewEmail;
        user.UserName = pending.NewEmail;
        user.NormalizedEmail = pending.NewEmail.ToUpperInvariant();
        user.NormalizedUserName = pending.NewEmail.ToUpperInvariant();
        user.EmailVerified = true;
        user.EmailConfirmed = true;
        user.SecurityStamp = Guid.NewGuid().ToString(); // invalidates all active sessions
        user.UpdatedAt = DateTime.UtcNow;
        pending.IsUsed = true;

        await _userRepository.UpdateAsync(user);
        await _pendingEmailRepository.UpdateAsync(pending);
        SendWelcomeEmail(user);
        _logger.LogInformation("Email confirmed for user {UserId} → {NewEmail}", request.UserId, pending.NewEmail);
        return Result.Success();
    }


    public async Task<Result<UserProfileResponse>> UpdateAvatarAsync(Guid userId, IFormFile avatar, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            return Result<UserProfileResponse>.Failure("User not found", 404);

        var uploadResult = await _fileService.ReplaceAsync(avatar, StorageDirectories.Avatars, user.AvatarUrl, ct);
        if (!uploadResult.IsSuccess)
        {
            _logger.LogError("Avatar upload failed for user {UserId}: {Errors}",
                userId, string.Join(", ", uploadResult.Errors));
            return Result<UserProfileResponse>.Failure(uploadResult.Errors);
        }

        user.AvatarUrl = uploadResult.Value!;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        _logger.LogInformation("Avatar updated for user {UserId}", userId);
        return Result<UserProfileResponse>.Success(MapToResponse(user));
    }
    // ── GDPR soft-delete ─────────────────────────────────────────────────────────

    public async Task<Result> DeleteAccountAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            return Result.Failure("User not found", 404);

        // Anonymize PII — retain the row for referential integrity (owned projects, etc.)
        user.FullName = null;
        user.Bio = null;
        user.AvatarUrl = null;
        user.Location = null;
        user.PhoneNumber = null;
        user.Email = $"deleted_{userId}@letopia.invalid";
        user.UserName = $"deleted_{userId}";
        user.NormalizedEmail = user.Email.ToUpperInvariant();
        user.NormalizedUserName = user.UserName.ToUpperInvariant();
        user.SecurityStamp = Guid.NewGuid().ToString(); // invalidates all tokens/sessions
        user.SocialLinks = new();
        user.PrivacySettings = new();
        user.NotificationPreferences = new();
        user.Skills = [];
        user.Interests = [];
        user.UpdatedAt = DateTime.UtcNow;

        // Revoke any pending email change tokens
        var pendingTokens = await _pendingEmailRepository
            .FindAsync(p => p.UserId == userId && !p.IsUsed);
        foreach (var token in pendingTokens)
        {
            token.IsUsed = true;
            await _pendingEmailRepository.UpdateAsync(token);
        }

        // TODO: cascade — transfer / archive owned projects, revoke memberships, etc.

        await _userRepository.UpdateAsync(user);

        _logger.LogWarning("Account anonymized (GDPR delete) for user {UserId}", userId);
        return Result.Success();
    }
    public async Task<Result<UserProfileResponse>> DeleteAvatarAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            return Result<UserProfileResponse>.Failure("User not found", 404);

        if (!string.IsNullOrEmpty(user.AvatarUrl))
            await _fileService.DeleteAsync(user.AvatarUrl, ct);

        user.AvatarUrl = null;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        _logger.LogInformation("Avatar deleted for user {UserId}", userId);
        return Result<UserProfileResponse>.Success(MapToResponse(user));
    }
    private void SendWelcomeEmail(User user)
    {
        var userName = user.FullName ?? EmailTemplates.DefaultUserName;

        _emailService.Enqueue(new EmailMessage(
            To: user.Email!,
            Subject: EmailTemplates.WelcomeSubject,
            Title: EmailTemplates.WelcomeTitle,
            Body: EmailTemplates.WelcomeBody,
            UserName: userName,
            IllustrationUrl: $"{_assetsBaseUrl}/{EmailTemplates.WelcomeIllustration}"
        ));
    }
    private static UserProfileResponse MapToResponse(User user) => new(
      Id: user.Id,
      FullName: user.FullName ?? string.Empty,
      Email: user.Email ?? string.Empty,
      Bio: user.Bio,
      PhoneNumber: user.PhoneNumber,
      AvatarUrl: user.AvatarUrl,
      Location: user.Location,
      Role: user.Role,
      EmailVerified: user.EmailVerified,
      TotalPoints: user.TotalPoints,
      CurrentStreak: user.CurrentStreak,
      LastLoginAt: user.LastLoginAt,
      CreatedAt: user.CreatedAt,
      NotificationPreferences: user.NotificationPreferences,
      SocialLinks: user.SocialLinks ?? [],
      Interests:user.Interests,
      Skills: user.Skills,
      PrivacySettings: user.PrivacySettings);
    private static string HashToken(string raw)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToBase64String(bytes);
    }
    private static PublicUserProfileResponse MapToPublicResponse(User user)
    {
        return new PublicUserProfileResponse(
            Id: user.Id,
            FullName: user.FullName ?? string.Empty,

            Email: user.PrivacySettings?.ShowEmailAddress == true
                ? user.Email
                : null,

            Role: user.Role.ToString(),

            // 👇 Privacy-aware phone
            PhoneNumber: user.PrivacySettings?.ShowPhoneNumber == true
                ? user.PhoneNumber
                : null,

            Bio: user.Bio,
            AvatarUrl: user.AvatarUrl,
            Location: user.Location,

            TotalPoints: user.TotalPoints,
            CurrentStreak: user.CurrentStreak,
            Skills:user.Skills,
            Interests:user.Interests,
            JoinedAt: user.CreatedAt,

            SocialLinks: user.SocialLinks
        );
    }
}
