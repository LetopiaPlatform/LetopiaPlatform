namespace LetopiaPlatform.Core.Exceptions;

/// <summary>
/// Base exception for all application-sepcific exceptions
/// </summary>
public class AppException : Exception
{
    public int StatusCode { get; }

    public AppException(string message, int statusCode = 500)
        :base(message)
    {
        StatusCode = statusCode;
    }
}
