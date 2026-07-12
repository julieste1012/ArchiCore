using ArchiCore.Domain.Common;

namespace ArchiCore.Domain.ValueObjects;

/// <summary>
/// Value Object que representa un código de barras utilizado para identificar
/// elementos físicos dentro del sistema.
/// </summary>
public sealed class Barcode : ValueObject
{
    private const int MinLength = 1;
    private const int MaxLength = 100;

    /// <summary>
    /// Valor del código de barras.
    /// </summary>
    public string Value { get; }

    private Barcode(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Crea una instancia válida de <see cref="Barcode"/> a partir del valor especificado.
    /// </summary>
    /// <param name="value">Valor del código de barras.</param>
    /// <returns>
    /// Un <see cref="Result{Barcode}"/> exitoso con la instancia creada, o fallido
    /// con el <see cref="Error"/> correspondiente si el valor no es válido.
    /// </returns>
    public static Result<Barcode> Create(string? value)
    {
        if (value is null)
        {
            return Result.Failure<Barcode>(
                new Error("Barcode.Null", "El código de barras es obligatorio."));
        }

        var trimmedValue = value.Trim();

        if (trimmedValue.Length == 0)
        {
            return Result.Failure<Barcode>(
                new Error("Barcode.Empty", "El código de barras no puede estar vacío."));
        }

        if (trimmedValue.Length < MinLength)
        {
            return Result.Failure<Barcode>(
                new Error("Barcode.TooShort", $"El código de barras debe tener al menos {MinLength} carácter."));
        }

        if (trimmedValue.Length > MaxLength)
        {
            return Result.Failure<Barcode>(
                new Error("Barcode.TooLong", $"El código de barras no puede superar los {MaxLength} caracteres."));
        }

        return Result.Success(new Barcode(trimmedValue));
    }

    /// <inheritdoc />
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    /// <inheritdoc />
    public override string ToString() => Value;
}
