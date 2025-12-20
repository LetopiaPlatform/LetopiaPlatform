namespace LetopiaPlatform.Core.Exceptions;

/// <summary>
/// Exception thrown when user authneitcated but lacks permissions
/// </summary>
public class ForbiddenException : AppException
{
    public ForbiddenException(string message = "Access Forbidden")
        : base(message, 403)
    {
    }
}