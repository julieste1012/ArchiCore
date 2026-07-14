using ArchiCore.Domain.Common;

namespace ArchiCore.Domain.ValueObjects;

/// <summary>
/// Value Object que representa una dirección física.
/// </summary>
public sealed class Address : CodeValueObject
{
    private const int MinLength = 5;
    private const int MaxLength = 250;

    private Address(string value)
        : base(value)
    {
    }

    /// <summary>
    /// Crea una instancia válida de <see cref="Address"/> a partir del valor especificado.
    /// </summary>
    /// <param name="value">Valor de la dirección física.</param>
    /// <returns>
    /// Un <see cref="Result{Address}"/> exitoso con la instancia creada, o fallido
    /// con el <see cref="Error"/> correspondiente si el valor no es válido.
    /// </returns>
    public static Result<Address> Create(string? value)
    {
        if (value is null)
        {
            return Result.Failure<Address>(
                new Error("Address.Null", "La dirección es obligatoria."));
        }

        var trimmedValue = value.Trim();

        if (trimmedValue.Length == 0)
        {
            return Result.Failure<Address>(
                new Error("Address.Empty", "La dirección no puede estar vacía."));
        }

        if (trimmedValue.Length < MinLength)
        {
            return Result.Failure<Address>(
                new Error("Address.TooShort", $"La dirección debe tener al menos {MinLength} caracteres."));
        }

        if (trimmedValue.Length > MaxLength)
        {
            return Result.Failure<Address>(
                new Error("Address.TooLong", $"La dirección no puede superar los {MaxLength} caracteres."));
        }

        return Result.Success(new Address(trimmedValue));
    }
}
