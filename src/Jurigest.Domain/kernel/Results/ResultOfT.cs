namespace Jurigest.Domain.Kernel.Results;

/// <summary>
/// Representa el resultado de una operación que devuelve un valor.
/// </summary>
/// <typeparam name="T">Tipo del valor.</typeparam>
public sealed class Result<T> : Result
{
    private Result(T value)
        : base(true, Error.None)
    {
        Value = value;
    }

    private Result(Error error)
        : base(false, error)
    {
        Value = default!;
    }

    public T Value { get; }

    public static Result<T> Success(T value)
        => new(value);

    public new static Result<T> Failure(Error error)
        => new(error);
}