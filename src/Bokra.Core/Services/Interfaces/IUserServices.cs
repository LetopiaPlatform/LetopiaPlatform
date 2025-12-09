using Bokra.Core.Common;
using Bokra.Core.DTOs.UserProfile.Request;
using Bokra.Core.DTOs.UserProfile.Response;
using Bokra.Core.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bokra.Core.Services.Interfaces
{
    public interface IUserServices
    {
        public Task<Result<ProfileResponse>> GetProfileAsync(Guid ProfileId);
        public Task<Result>UpdateProfileAsync(UpdatedProfileRequest request);
        public Task<Result<string>> UploadAvatarAsync(AvatarRequest request);
    }
}
