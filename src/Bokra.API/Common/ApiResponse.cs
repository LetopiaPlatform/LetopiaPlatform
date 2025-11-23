namespace Bokra.API.Common;

/// <summary>
/// Standard API response wrapper for consistent responses
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse<T> SuccessResponse(T data, string message = "Success")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static ApiResponse<T> FailureResponse(string error)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = error,
            Errors = new List<string> { error }
        };
    }

    public static ApiResponse<T> FailureResponse(List<string> errors)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = "Operation failed",
            Errors = errors
        };
    }
}