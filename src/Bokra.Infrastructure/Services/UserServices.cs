using Bokra.Core.Common;
using Bokra.Core.DTOs.Auth.Response;
using Bokra.Core.DTOs.UserProfile.Request;
using Bokra.Core.DTOs.UserProfile.Response;
using Bokra.Core.Entities.Identity;
using Bokra.Core.Interfaces;
using Bokra.Core.Services.Interfaces;
using Bokra.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bokra.Infrastructure.Services
{
    public class UserServices : IUserServices
    {
        private readonly UserManager<User> _userManager;
        private readonly IGenericRepository<User> _genericRepository;
        private readonly IUnitOfWork<ApplicationDbContext> _unitOfWork;
        private readonly IFileStorageService _fileStorageService;

        public UserServices(UserManager<User> userManager, IGenericRepository<User> genericRepository, IUnitOfWork<ApplicationDbContext> unitOfWork, IFileStorageService fileStorageService)
        {
            _userManager = userManager;
            _genericRepository = genericRepository;
            _unitOfWork = unitOfWork;
            _fileStorageService = fileStorageService;
        }

        public async Task<Result<ProfileResponse>> GetProfileAsync(Guid ProfileId)
        {
            var existingUser = await _userManager.FindByIdAsync(ProfileId.ToString());
            if (existingUser == null)
                return Result<ProfileResponse>.Failure("User with this Id NotFound.", 404);

            var response = new ProfileResponse(
                FullName: existingUser.FullName, Bio: existingUser.Bio,
                AvatarUrl: existingUser.AvatarUrl, Role: existingUser.Role,
                TotalPoints: existingUser.TotalPoints);

            return Result<ProfileResponse>.Success(response);
        }

        public async Task<Result> UpdateProfileAsync(UpdatedProfileRequest request)
        {
            var existingUser = await _userManager.FindByIdAsync(request.Id.ToString());
            if (existingUser == null)
                return Result.Failure("User with this Id NotFound.", 404);

            existingUser.Bio = request.Bio;
            existingUser.FullName = request.FullName;
            existingUser.UpdatedAt = DateTime.UtcNow;
            existingUser.DateOfBirth = request.DateOfBirth;

            await _genericRepository.UpdateAsync(existingUser);
            await _unitOfWork.SaveChangesAsync();
            return Result.Success();
        }

        public async Task<Result<string>> UploadAvatarAsync(AvatarRequest request)
        {
            var existingUser = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (existingUser == null)
                return Result<string>.Failure("User not found.", 404);

            if (request.Avatar == null || request.Avatar.Length == 0)
                return Result<string>.Failure("No avatar file uploaded.");

            
            var result = await _fileStorageService.UploadAsync(request.Avatar, "avatars");

            if (!result.IsSuccess)
                return Result<string>.Failure(result.Errors);

            existingUser.AvatarUrl = result.Value;
            await _genericRepository.UpdateAsync(existingUser);
            await _unitOfWork.SaveChangesAsync();

            return Result<string>.Success(result.Value);
        }

    }
}
