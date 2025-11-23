using FluentValidation;

namespace Bokra.Core.Common;

public class Result
{
    public bool IsSuccess { get; protected set; }
    public string Error { get; protected set; } = string.Empty;
    public List<string> Errors { get; protected set; } = new();

    protected Result(bool isSuccess, string error)
    {
        this.IsSuccess = isSuccess;
        Error = error;

        if (!string.IsNullOrEmpty(Error))
        {
            Errors.Add(error);
        }
    }

    protected Result(bool isSuccess, List<string> errors)
    {
        IsSuccess = isSuccess;
        Errors = errors;
        Error = errors.Count > 0 ? errors[0] : string.Empty;
    }

    public static Result Success() => new(true, string.Empty);
    public static Result Failure(string error) => new(false, error);
    public static Result Failure(List<string> errors) => new(false, errors);
}

public class Result<T> : Result
{
    public T? Value { get; private set; }

    private Result(bool isSuccess, T? value, string error)
        :base(isSuccess, error)
    {
        Value = value;
    }

    private Result(bool isSuccess, T? value, List<string> errors)
        :base(isSuccess, errors)
    {
        Value = value;
    }

    public static Result<T> Success(T value) => new(true, value, string.Empty);
    public new static Result<T> Failure(string error) => new(false, default, error);
    public new static Result<T> Failure(List<string> errors) => new(false, default, errors);
}