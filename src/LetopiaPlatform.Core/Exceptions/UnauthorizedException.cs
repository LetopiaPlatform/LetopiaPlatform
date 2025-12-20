namespace LetopiaPlatform.Core.Exceptions;

/// <summary>
/// Exception thrown when user is not authorized
/// </summary>
public class UnauthorizedException : AppException
{
    public UnauthorizedException(string message = "Unauthorized access")
        : base(message, 401)
    {
    }
}
