namespace LetopiaPlatform.Core.Exceptions;

/// <summary>
/// Exception thorwn when validation fails
/// </summary>
public class ValidationException : AppException
{
    public List<string> ValidationErrors { get; }
    public ValidationException(string message)
        :base(message, 400)
    {
        ValidationErrors = new List<string> { message };
    }

    public ValidationException(List<string> errors)
        :base("Validation failed", 400)
    {
        ValidationErrors = errors;
    }
}

