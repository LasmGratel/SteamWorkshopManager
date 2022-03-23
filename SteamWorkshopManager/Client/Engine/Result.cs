using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace SteamWorkshopManager.Client.Engine;

public record Result<T>
{
    public T GetOrThrow()
    {
        return this switch
        {
            Success(var content) => content,
            Failure(var cause) => throw cause ?? new Exception("This is an exception thrown by Result.Failure"),
            _ => throw new Exception("Invalid derived type of Result<T>")
        };
    }
    
    public T? GetOrElse(T? @else)
    {
        return this switch
        {
            Success(var content) => content,
            Failure => @else,
            _ => throw new Exception("Invalid derived type of Result<T>")
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T> OfSuccess(T value)
    {
        return new Success(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T> OfFailure(Exception? cause = null)
    {
        return new Failure(cause);
    }

    public Result<R> Bind<R>(Func<T, R> selector)
    {
        return this switch
        {
            Success(var content) => Result<R>.OfSuccess(selector(content)),
            Failure(var cause) => Result<R>.OfFailure(cause),
            _ => throw new Exception("Invalid derived type of Result<T>")
        };
    }

    public async Task<Result<R>> BindAsync<R>(Func<T, Task<R>> selector)
    {
        return this switch
        {
            Success(var content) => Result<R>.OfSuccess(await selector(content)),
            Failure(var cause) => Result<R>.OfFailure(cause),
            _ => throw new Exception("Invalid derived type of Result<T>")
        };
    }

    public static Result<R?> Wrap<R>(Result<T> result) where R : class
    {
        return result.Bind(t => t as R);
    }

    
    public record Success(T Value) : Result<T>
    {
        public void Deconstruct(out T value)
        {
            value = Value;
        }
    }

    
    public record Failure(Exception? Cause) : Result<T>
    {
        public void Deconstruct(out Exception? cause)
        {
            cause = Cause;
        }
    }
}