using System.Diagnostics.CodeAnalysis;

namespace BlazorServerFunctions.Generator.Models;

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    public Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None ||
            !isSuccess && error == Error.None)
        {
            throw new ArgumentException("Invalid error", nameof(error));
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() =>
        new(true, Error.None);

    public static Result<T> Success<T>(T value) =>
        new(value, true, Error.None);

    public static Result Failure(Error error) =>
        new(false, error);

    public static Result<T> Failure<T>(Error error) =>
        new(default, false, error);
}

public sealed class Result<T> : Result
{
    private readonly T? _value;

    public Result(T? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    [NotNull]
    public T Value =>
        IsSuccess
            ? _value!
            : throw new InvalidOperationException("Cannot access value of a failed result.");

    public static implicit operator Result<T>(T? value) =>
        value is not null
            ? Success(value)
            : Failure<T>(Error.NullValue);
}