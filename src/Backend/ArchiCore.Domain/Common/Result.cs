namespace ArchiCore.Domain.Common;

/// <summary>
/// Representa un error de dominio, identificado por un código y un mensaje descriptivo.
/// </summary>
public sealed class Error : IEquatable<Error>
{
    /// <summary>
    /// Instancia que representa la ausencia de error.
    /// </summary>
    public static readonly Error None = new(string.Empty, string.Empty);

    /// <summary>
    /// Código único que identifica el tipo de error.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Mensaje descriptivo del error.
    /// </summary>
    public string Message { get; }

    public Error(string code, string message)
    {
        Code = code;
        Message = message;
    }

    public bool Equals(Error? other)
    {
        if (other is null)
        {
            return false;
        }

        return Code == other.Code && Message == other.Message;
    }

    public override bool Equals(object? obj)
    {
        return obj is Error other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Code, Message);
    }

    public static bool operator ==(Error? left, Error? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Error? left, Error? right)
    {
        return !Equals(left, right);
    }
}

/// <summary>
/// Representa el resultado de una operación de dominio, indicando éxito o fracaso
/// sin recurrir al uso de excepciones para el control de flujo.
/// </summary>
public class Result
{
    /// <summary>
    /// Indica si la operación fue exitosa.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Indica si la operación falló.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Error asociado a la operación en caso de fallo.
    /// </summary>
    public Error Error { get; }

    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
        {
            throw new InvalidOperationException("Un resultado exitoso no puede contener un error.");
        }

        if (!isSuccess && error == Error.None)
        {
            throw new InvalidOperationException("Un resultado fallido debe contener un error.");
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// Crea un resultado exitoso.
    /// </summary>
    public static Result Success() => new(true, Error.None);

    /// <summary>
    /// Crea un resultado fallido con el error especificado.
    /// </summary>
    /// <param name="error">Error que describe la causa del fallo.</param>
    public static Result Failure(Error error) => new(false, error);

    /// <summary>
    /// Crea un resultado exitoso que contiene un valor.
    /// </summary>
    /// <param name="value">Valor asociado al resultado.</param>
    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);

    /// <summary>
    /// Crea un resultado fallido tipado con el error especificado.
    /// </summary>
    /// <param name="error">Error que describe la causa del fallo.</param>
    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);
}

/// <summary>
/// Representa el resultado de una operación de dominio que produce un valor en caso de éxito.
/// </summary>
/// <typeparam name="TValue">Tipo del valor producido por la operación.</typeparam>
public sealed class Result<TValue> : Result
{
    private readonly TValue? _value;

    internal Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    /// <summary>
    /// Valor producido por la operación cuando esta fue exitosa.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Se lanza si se intenta acceder al valor cuando el resultado representa un fallo.
    /// </exception>
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("No es posible acceder al valor de un resultado fallido.");

    public static implicit operator Result<TValue>(TValue value) => Success(value);
}
