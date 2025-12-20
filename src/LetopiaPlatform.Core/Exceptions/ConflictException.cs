namespace LetopiaPlatform.Core.Exceptions;

/// <summary>
/// Exception thorwn when there's a conflict (e.g., duplidate email)
/// </summary>
public class ConflictException : AppException
{
    public ConflictException(string message)
        : base(message, 409)
    {
    }
}