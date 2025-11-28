
using Bokra.Core.Common;
using Bokra.Core.DTOs.Auth.Request;
using Bokra.Core.DTOs.Auth.Response;

namespace Bokra.Core.Services.Interfaces
{
    public interface IAuthService
    {
        public Task<Result<AuthResponse>> SignUpAsync(SignUpRequest request);
        public Task<Result<AuthResponse>> LoginAsync(LoginRequest request);
    }
}
