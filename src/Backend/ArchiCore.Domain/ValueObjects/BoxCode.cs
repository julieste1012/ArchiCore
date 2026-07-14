using ArchiCore.Domain.Common;

namespace ArchiCore.Domain.ValueObjects;

/// <summary>
/// Value Object que representa el código identificador de una caja,
/// con formato "BOX-000001".
/// </summary>
public sealed class BoxCode : CodeValueObject
{
    private const int MaxLength = 20;
    private const string Prefix = "BOX-";

    private BoxCode(string value)
        : base(value)
    {
    }

    /// <summary>
    /// Crea una instancia válida de <see cref="BoxCode"/> a partir del valor especificado.
    /// </summary>
    /// <param name="value">Valor del código de caja.</param>
    /// <returns>
    /// Un <see cref="Result{BoxCode}"/> exitoso con la instancia creada, o fallido
    /// con el <see cref="Error"/> correspondiente si el valor no es válido.
    /// </returns>
    public static Result<BoxCode> Create(string? value)
    {
        if (value is null)
        {
            return Result.Failure<BoxCode>(
                new Error("BoxCode.Null", "El código de caja es obligatorio."));
        }

        var trimmedValue = value.Trim();

        if (trimmedValue.Length == 0)
        {
            return Result.Failure<BoxCode>(
                new Error("BoxCode.Empty", "El código de caja no puede estar vacío."));
        }

        if (trimmedValue.Length > MaxLength)
        {
            return Result.Failure<BoxCode>(
                new Error("BoxCode.TooLong", $"El código de caja no puede superar los {MaxLength} caracteres."));
        }

        if (!trimmedValue.StartsWith(Prefix, StringComparison.Ordinal))
        {
            return Result.Failure<BoxCode>(
                new Error("BoxCode.InvalidFormat", $"El código de caja debe iniciar con \"{Prefix}\"."));
        }

        return Result.Success(new BoxCode(trimmedValue));
    }
}
