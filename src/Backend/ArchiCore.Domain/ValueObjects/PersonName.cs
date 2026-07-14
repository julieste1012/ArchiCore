using ArchiCore.Domain.Common;

namespace ArchiCore.Domain.ValueObjects;

/// <summary>
/// Value Object que representa el nombre completo de una persona.
/// </summary>
public sealed class PersonName : CodeValueObject
{
    private const int MinLength = 3;
    private const int MaxLength = 150;

    private PersonName(string value)
        : base(value)
    {
    }

    /// <summary>
    /// Crea una instancia válida de <see cref="PersonName"/> a partir del valor especificado.
    /// </summary>
    /// <param name="value">Valor del nombre completo.</param>
    /// <returns>
    /// Un <see cref="Result{PersonName}"/> exitoso con la instancia creada, o fallido
    /// con el <see cref="Error"/> correspondiente si el valor no es válido.
    /// </returns>
    public static Result<PersonName> Create(string? value)
    {
        if (value is null)
        {
            return Result.Failure<PersonName>(
                new Error("PersonName.Null", "El nombre es obligatorio."));
        }

        var trimmedValue = value.Trim();

        if (trimmedValue.Length == 0)
        {
            return Result.Failure<PersonName>(
                new Error("PersonName.Empty", "El nombre no puede estar vacío."));
        }

        if (trimmedValue.Length < MinLength)
        {
            return Result.Failure<PersonName>(
                new Error("PersonName.TooShort", $"El nombre debe tener al menos {MinLength} caracteres."));
        }

        if (trimmedValue.Length > MaxLength)
        {
            return Result.Failure<PersonName>(
                new Error("PersonName.TooLong", $"El nombre no puede superar los {MaxLength} caracteres."));
        }

        return Result.Success(new PersonName(trimmedValue));
    }
}
