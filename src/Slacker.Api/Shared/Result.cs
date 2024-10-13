using System.Diagnostics.CodeAnalysis;

namespace Slacker.Api.Shared;

#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable SA1302 // Interface names should begin with I
public interface Success { }

public interface Failure
{
    string Message { get; }

    IReadOnlyCollection<Error> Errors { get; }
}
#pragma warning restore SA1302 // Interface names should begin with I
#pragma warning restore IDE1006 // Naming Styles

public abstract class Result<T>
{
    private T value;

    protected bool Succeeded { get; set; }

    public Error Error { get; protected set; }

    protected Result(T data)
    {
        Value = data;
    }

    protected Result(Error error)
    {
        Error = error;
    }

    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Error))]
    public T Value
    {
        get => Succeeded ? value : throw new InvalidOperationException($"You can't access .{nameof(Value)} when .{nameof(Succeeded)} is false");
        set => this.value = value;
    }

    public static Result<T> Success(T data) => new SuccessResult<T>(data);

    public static Result<T> Failure(string details) => new ErrorResult<T>(details);

    public static Result<T> Failure(Error error) => new ErrorResult<T>(error);

    public static Result<T> Failure(string message, IReadOnlyCollection<Error> errors) => new ErrorResult<T>(message, errors);

    public static implicit operator Result<T>(T value) => new SuccessResult<T>(value);

    public static implicit operator Result<T>(Error error) => new ErrorResult<T>(error);

    public static implicit operator T(Result<T> result) => result.Value;

    public TResult Switch<TResult>(Func<T, TResult> onSuccess, Func<Error, TResult> onFailure)
    {
        return Succeeded ? onSuccess(Value!) : onFailure(Error);
    }
}

public abstract class Result : Result<int>
{
    protected Result()
        : base(0)
    {
    }

    public static Result Success() => new SuccessResult();

    public static Result<T> Success<T>(T value) => new SuccessResult<T>(value);

    public static Result<T> Failure<T>(T value) => new ErrorResult<T>(new Error("value"));
}

public class SuccessResult : Result, Success
{
    public SuccessResult()
    {
        Succeeded = true;
    }
}

public class SuccessResult<T> : Result<T>, Success
{
    public SuccessResult(T data)
        : base(data)
    {
        Succeeded = true;
    }
}

public class ErrorResult<T> : Result<T>, Failure
{
    public ErrorResult(Error error)
        : base(error)
    {
        Errors = [error];
        Message = error.Details;
    }

    public ErrorResult(string message)
        : this(message, [])
    {
    }

    public ErrorResult(string message, IReadOnlyCollection<Error> errors)
        : base(new Error(message))
    {
        Message = message;
        Succeeded = false;
        Errors = errors ?? [];
    }

    public string Message { get; set; }

    public IReadOnlyCollection<Error> Errors { get; }
}

public class Error(string code, string details)
{
    public Error(string details)
        : this(string.Empty, details)
    {
    }

    public string Code { get; } = code ?? string.Empty;

    public string Details { get; } = details;
}

public static class ResultExtensions
{
    public static T? Value<T>(this Result<T> result)
    {
        return result switch
        {
            SuccessResult<T> success => success.Value,
            _ => default
        };
    }

    public static Result<T> Ok<T>(T value) => new SuccessResult<T>(value);

    /// <summary>
    /// Match the result with the provided functions which allows to execute different logic based on the result.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <param name="result">The result to match.</param>
    /// <param name="onSuccess">The function to execute when the result is successful.</param>
    /// <param name="onFailure">The function to execute when the result is a failure.</param>
    public static T Match<T>(this Result<T> result, Func<T> onSuccess, Func<Error, T> onFailure)
    {
        return result is Success ? onSuccess() : onFailure(result.Error);
    }
}
