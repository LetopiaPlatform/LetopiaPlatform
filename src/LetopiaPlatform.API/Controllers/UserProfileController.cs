using LetopiaPlatform.API.AppMetaData;
using LetopiaPlatform.API.DTOs.User;
using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.Entities.Identity;
using LetopiaPlatform.Core.Interfaces;
using LetopiaPlatform.Core.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace LetopiaPlatform.API.Controllers
{


    [Authorize]
    [Route("api/[controller]")]

    public class UserProfileController : BaseController
    {
        private readonly IGenericRepository<User> _userRepository;
        private readonly IFileStorageService _fileService;

        public UserProfileController(IGenericRepository<User> userRepository, IFileStorageService fileService)
        {
            _userRepository = userRepository;
            _fileService = fileService;
        }

        /// <summary>
        /// Get current user info
        /// </summary>
        [HttpGet(Router.User.Me)]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = GetUserId();
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return HandleResult(Result.Failure("User not found"));

            var response = new UserProfileResponseDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Bio = user.Bio,
                PhoneNumber = user.PhoneNumber,
                DateOfBirth = user.DateOfBirth,
                AvatarUrl = user.AvatarUrl
            };

            return HandleResult(Result<UserProfileResponseDto>.Success(response));
        }

        /// <summary>
        /// Update user profile details
        /// </summary>
        [HttpPut(Router.User.Update)]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateUserProfileDto dto)
        {
            // ????? ????? FluentValidation ??? Result
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                                       .SelectMany(v => v.Errors)
                                       .Select(e => e.ErrorMessage)
                                       .ToList();
                return HandleResult(Result.Failure(errors));
            }

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
                var result = await _fileService.ReplaceAsync(dto.AvatarUrl, "avatars", user.AvatarUrl);
                if (!result.IsSuccess) return HandleResult(result);

                user.AvatarUrl = result.Value!;
            }

            await _userRepository.UpdateAsync(user);

            var response = new UserProfileResponseDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Bio = user.Bio,
                PhoneNumber = user.PhoneNumber,
                DateOfBirth = user.DateOfBirth,
                AvatarUrl = user.AvatarUrl
            };

            return HandleResult(Result<UserProfileResponseDto>.Success(response));
        }
    }


}
