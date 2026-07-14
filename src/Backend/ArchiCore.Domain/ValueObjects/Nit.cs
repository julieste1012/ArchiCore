using ArchiCore.Domain.Common;

namespace ArchiCore.Domain.ValueObjects;

/// <summary>
/// Value Object que representa el Número de Identificación Tributaria (NIT) colombiano.
/// </summary>
public sealed class Nit : CodeValueObject
{
    private const int MaxLength = 20;

    private Nit(string value)
        : base(value)
    {
    }

    /// <summary>
    /// Crea una instancia válida de <see cref="Nit"/> a partir del valor especificado.
    /// </summary>
    /// <param name="value">Valor del NIT.</param>
    /// <returns>
    /// Un <see cref="Result{Nit}"/> exitoso con la instancia creada, o fallido
    /// con el <see cref="Error"/> correspondiente si el valor no es válido.
    /// </returns>
    public static Result<Nit> Create(string? value)
    {
        if (value is null)
        {
            return Result.Failure<Nit>(
                new Error("Nit.Null", "El NIT es obligatorio."));
        }

        var trimmedValue = value.Trim();

        if (trimmedValue.Length == 0)
        {
            return Result.Failure<Nit>(
                new Error("Nit.Empty", "El NIT no puede estar vacío."));
        }

        if (trimmedValue.Length > MaxLength)
        {
            return Result.Failure<Nit>(
                new Error("Nit.TooLong", $"El NIT no puede superar los {MaxLength} caracteres."));
        }

        return Result.Success(new Nit(trimmedValue));
    }
}
