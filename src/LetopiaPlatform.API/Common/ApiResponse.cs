namespace LetopiaPlatform.API.Common;

/// <summary>
/// Standard API response wrapper for consistent responses
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse<T> SuccessResponse(T data, string message = "Success", int statusCode = 200)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            StatusCode = statusCode
        };
    }

    public static ApiResponse<T> FailureResponse(string error, int statusCode = 400)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = error,
            Errors = new List<string> { error },
            StatusCode = statusCode
        };
    }

    public static ApiResponse<T> FailureResponse(List<string> errors, int statusCode = 400)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = "Failed",
            Errors = errors,
            StatusCode = statusCode
        };
    }
}