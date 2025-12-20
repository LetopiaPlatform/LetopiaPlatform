using FluentValidation;

namespace LetopiaPlatform.Core.Common;

public class Result
{
    public bool IsSuccess { get; protected set; }
    public string Error { get; protected set; } = string.Empty;
    public List<string> Errors { get; protected set; } = new();
    public int StatusCode { get; set; } = 200;
    protected Result(bool isSuccess, string error, int statusCode = 200)
    {
        this.IsSuccess = isSuccess;
        Error = error;
        StatusCode = statusCode;

        if (!string.IsNullOrEmpty(Error))
        {
            Errors.Add(error);
        }
    }

    protected Result(bool isSuccess, List<string> errors, int statusCode = 400)
    {
        IsSuccess = isSuccess;
        Errors = errors;
        Error = errors.FirstOrDefault() ?? string.Empty;
        StatusCode = statusCode;
    }

    // Success factory method
    public static Result Success(int statusCode = 200) => new(true, string.Empty, statusCode);

    // Failure factory methods
    public static Result Failure(string error, int statusCode = 400) => new(false, error, statusCode);
    public static Result Failure(List<string> errors, int statusCode = 400) => new(false, errors, statusCode);
}

public class Result<T> : Result
{
    public T? Value { get; private set; }

    private Result(bool isSuccess, T? value, string error, int statusCode = 200)
        :base(isSuccess, error, statusCode)
    {
        Value = value;
    }

    private Result(bool isSuccess, T? value, List<string> errors, int statusCode = 400)
        :base(isSuccess, errors, statusCode)
    {
        Value = value;
    }

    // Success factory method
    public static Result<T> Success(T value, int statusCode = 200) => new(true, value, string.Empty, statusCode);

    // Failure factory methods
    public new static Result<T> Failure(string error, int statusCode = 400) => new(false, default, error, statusCode);
    public new static Result<T> Failure(List<string> errors, int statusCode = 400) => new(false, default, errors, statusCode);
}