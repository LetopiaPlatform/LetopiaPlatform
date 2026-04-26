using LetopiaPlatform.Core.Common;
using LetopiaPlatform.Core.DTOs.Auth.Request;
using LetopiaPlatform.Core.DTOs.Auth.Response;
using LetopiaPlatform.Core.DTOs.UserRefershToken.Request;

namespace LetopiaPlatform.Core.Services.Interfaces;

/// <summary>
/// Handles user authentication operations including registration and sign-in.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Registers a new user account and sends a verification code to the user's email.
    /// </summary>
<<<<<<< HEAD
=======
    /// <param name="request">The sign-up details including email, password, and profile info.</param>
    /// <returns>A result indicating the success or failure of the registration.</returns>
>>>>>>> 1ffb18be0816185e446b8c619cbe640eb130eafc
    Task<Result> SignUpAsync(SignUpRequest request);

    /// <summary>
    /// Authenticates a user with their credentials and returns an authentication token.
    /// </summary>
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Authenticates a user using their Google account and returns an authentication token.
    /// </summary>
    Task<Result<AuthResponse>> GoogleLoginAsync(GoogleLoginRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates an expired JWT and a refresh token to issue a new pair of tokens.
    /// </summary>
    Task<Result<AuthResponse>> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a verification code to the user's email for purposes such as email verification or password reset.
    /// </summary>
<<<<<<< HEAD
=======
    Task<Result> SendVerificationCodeAsync(SendCodeRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies the code sent to the user's email and returns an authentication response on success.
    /// </summary>
<<<<<<< HEAD
<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
>>>>>>> main
>>>>>>> 2c462ddb241916e75612c627eecded6fbfcbe955
    /// <param name="request">The request containing the user's email and the purpose of the verification code.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A result indicating the success or failure of the operation.</returns>
    Task<Result> SendVerificationCodeAsync(SendCodeRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies the code sent to the user's email for a specific purpose (e.g., email verification, password reset).
    /// </summary>
    /// <param name="request">The request containing the user's email, the verification code, and the purpose of the verification.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
<<<<<<< HEAD
>>>>>>> 1ffb18be0816185e446b8c619cbe640eb130eafc
=======
>>>>>>> 97493235b7350e5bec383d0f8807bb1a6b77d66a
=======
<<<<<<< HEAD
=======
>>>>>>> 1ffb18be0816185e446b8c619cbe640eb130eafc
=======
>>>>>>> 97493235b7350e5bec383d0f8807bb1a6b77d66a
>>>>>>> main
>>>>>>> 2c462ddb241916e75612c627eecded6fbfcbe955
    Task<Result<AuthResponse>> VerifyEmailAsync(VerifyEmailRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a password reset code to the user's email.
    /// </summary>
<<<<<<< HEAD
=======
    Task<Result> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resets the user's password after verifying the provided code.
    /// </summary>
    Task<Result> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default);
<<<<<<< HEAD

    /// <summary>
    /// Validates an expired JWT and a refresh token to issue a new pair of tokens.
    /// </summary>
    Task<Result<AuthResponse>> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken = default);
<<<<<<< HEAD
=======
<<<<<<< HEAD
    /// <param name="request">The request containing the user's email.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A result indicating the success or failure of the password reset operation.</returns>
=======
>>>>>>> main
    /// <param name="request">The request containing the user's email.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A result indicating the success or failure of the operation.</returns>
>>>>>>> 2c462ddb241916e75612c627eecded6fbfcbe955
    Task<Result> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resets the user's password after verifying the provided code. The user must have received a verification code for password reset prior to calling this method.
    /// </summary>
    /// <param name="request">The request containing the user's email, the verification code, and the new password.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A result indicating the success or failure of the password reset operation.</returns>
    Task<Result> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default);
<<<<<<< HEAD
>>>>>>> 1ffb18be0816185e446b8c619cbe640eb130eafc
=======
>>>>>>> 97493235b7350e5bec383d0f8807bb1a6b77d66a
=======
<<<<<<< HEAD

    /// <summary>
    /// Validates an expired JWT and a refresh token to issue a new pair of tokens.
    /// </summary>
    Task<Result<AuthResponse>> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken = default);
=======
>>>>>>> 1ffb18be0816185e446b8c619cbe640eb130eafc
=======
>>>>>>> 97493235b7350e5bec383d0f8807bb1a6b77d66a
>>>>>>> main
>>>>>>> 2c462ddb241916e75612c627eecded6fbfcbe955
}
