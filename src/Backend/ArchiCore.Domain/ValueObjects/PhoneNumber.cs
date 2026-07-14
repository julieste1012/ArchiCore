using System.Text.RegularExpressions;
using ArchiCore.Domain.Common;

namespace ArchiCore.Domain.ValueObjects;

/// <summary>
/// Value Object que representa un número telefónico.
/// </summary>
public sealed partial class PhoneNumber : CodeValueObject
{
    private const int MinLength = 7;
    private const int MaxLength = 20;

    private PhoneNumber(string value)
        : base(value)
    {
    }

    /// <summary>
    /// Crea una instancia válida de <see cref="PhoneNumber"/> a partir del valor especificado.
    /// </summary>
    /// <param name="value">Valor del número telefónico.</param>
    /// <returns>
    /// Un <see cref="Result{PhoneNumber}"/> exitoso con la instancia creada, o fallido
    /// con el <see cref="Error"/> correspondiente si el valor no es válido.
    /// </returns>
    public static Result<PhoneNumber> Create(string? value)
    {
        if (value is null)
        {
            return Result.Failure<PhoneNumber>(
                new Error("PhoneNumber.Null", "El número telefónico es obligatorio."));
        }

        var trimmedValue = value.Trim();

        if (trimmedValue.Length == 0)
        {
            return Result.Failure<PhoneNumber>(
                new Error("PhoneNumber.Empty", "El número telefónico no puede estar vacío."));
        }

        if (trimmedValue.Length < MinLength)
        {
            return Result.Failure<PhoneNumber>(
                new Error("PhoneNumber.TooShort", $"El número telefónico debe tener al menos {MinLength} caracteres."));
        }

        if (trimmedValue.Length > MaxLength)
        {
            return Result.Failure<PhoneNumber>(
                new Error("PhoneNumber.TooLong", $"El número telefónico no puede superar los {MaxLength} caracteres."));
        }

        if (!AllowedCharactersRegex().IsMatch(trimmedValue))
        {
            return Result.Failure<PhoneNumber>(
                new Error("PhoneNumber.InvalidFormat", "El número telefónico contiene caracteres no permitidos."));
        }

        return Result.Success(new PhoneNumber(trimmedValue));
    }

    [GeneratedRegex(@"^[0-9+\-\s()]+$")]
    private static partial Regex AllowedCharactersRegex();
}
