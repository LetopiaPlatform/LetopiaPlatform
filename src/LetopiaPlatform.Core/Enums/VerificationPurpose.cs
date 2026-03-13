namespace LetopiaPlatform.Core.Enums;

public enum VerificationPurpose
{
    /// <summary>
    /// Used for verifying a user's email address during registration or when updating their email.
    /// </summary>
    EmailVerification,

    /// <summary>
    /// Used for verifying a user's identity when they request a password reset.
    /// </summary>
    PasswordReset
}
