using System.Diagnostics.CodeAnalysis;

namespace Claims.Domain.Core.Primitives;

public class Result<T>
{
    private readonly T? _value;

    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess { get; }

    [MemberNotNullWhen(false, nameof(Value))]
    [MemberNotNullWhen(true, nameof(Error))]
    public bool IsFailure => !IsSuccess;

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failure result cannot be accessed.");

    public string? Error { get; }

    protected Result(T? value, bool isSuccess, string? error)
    {
        _value = value;
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result<T> Success(T value) => new(value, true, null);

    public static Result<T> Failure(string error) => new(default, false, error);

    public static implicit operator Result<T>(T value) => Success(value);
}

public static class Result
{
    public static Result<T> Success<T>(T value) => Result<T>.Success(value);

    public static Result<T> Failure<T>(string error) => Result<T>.Failure(error);
}
