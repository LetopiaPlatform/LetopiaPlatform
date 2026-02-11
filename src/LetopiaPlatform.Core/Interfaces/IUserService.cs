using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.DTOs.User;
using Microsoft.AspNetCore.Http;

namespace LetopiaPlatform.Core.Services.Interfaces;

public interface IUserService
{
    Task<Result<UserProfileResponse>> GetProfileAsync(Guid userId, CancellationToken ct = default);
    Task<Result<UserProfileResponse>> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, IFormFile? avatar, CancellationToken ct = default);
}
