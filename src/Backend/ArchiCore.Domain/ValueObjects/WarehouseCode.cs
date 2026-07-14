using ArchiCore.Domain.Common;

namespace ArchiCore.Domain.ValueObjects;

/// <summary>
/// Value Object que representa el código identificador de una bodega,
/// con formato "WH-001".
/// </summary>
public sealed class WarehouseCode : CodeValueObject
{
    private const int MaxLength = 20;
    private const string Prefix = "WH-";

    private WarehouseCode(string value)
        : base(value)
    {
    }

    /// <summary>
    /// Crea una instancia válida de <see cref="WarehouseCode"/> a partir del valor especificado.
    /// </summary>
    /// <param name="value">Valor del código de bodega.</param>
    /// <returns>
    /// Un <see cref="Result{WarehouseCode}"/> exitoso con la instancia creada, o fallido
    /// con el <see cref="Error"/> correspondiente si el valor no es válido.
    /// </returns>
    public static Result<WarehouseCode> Create(string? value)
    {
        if (value is null)
        {
            return Result.Failure<WarehouseCode>(
                new Error("WarehouseCode.Null", "El código de bodega es obligatorio."));
        }

        var trimmedValue = value.Trim();

        if (trimmedValue.Length == 0)
        {
            return Result.Failure<WarehouseCode>(
                new Error("WarehouseCode.Empty", "El código de bodega no puede estar vacío."));
        }

        if (trimmedValue.Length > MaxLength)
        {
            return Result.Failure<WarehouseCode>(
                new Error("WarehouseCode.TooLong", $"El código de bodega no puede superar los {MaxLength} caracteres."));
        }

        if (!trimmedValue.StartsWith(Prefix, StringComparison.Ordinal))
        {
            return Result.Failure<WarehouseCode>(
                new Error("WarehouseCode.InvalidFormat", $"El código de bodega debe iniciar con \"{Prefix}\"."));
        }

        return Result.Success(new WarehouseCode(trimmedValue));
    }
}
