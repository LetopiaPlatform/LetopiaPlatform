
using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.DTOs.Auth.Request;
using LetopiaPlatform.Core.DTOs.Auth.Response;

namespace LetopiaPlatform.Core.Services.Interfaces
{
    public interface IAuthService
    {
        public Task<Result<AuthResponse>> SignUpAsync(SignUpRequest request);
        public Task<Result<AuthResponse>> LoginAsync(LoginRequest request);
    }
}
