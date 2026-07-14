using ArchiCore.Domain.Common;

namespace ArchiCore.Domain.ValueObjects;

/// <summary>
/// Value Object que representa el número identificador de un contrato,
/// con formato temporal "CTR-2026-000001".
/// </summary>
public sealed class ContractNumber : CodeValueObject
{
    private const int MaxLength = 20;
    private const string Prefix = "CTR-";

    private ContractNumber(string value)
        : base(value)
    {
    }

    /// <summary>
    /// Crea una instancia válida de <see cref="ContractNumber"/> a partir del valor especificado.
    /// </summary>
    /// <param name="value">Valor del número de contrato.</param>
    /// <returns>
    /// Un <see cref="Result{ContractNumber}"/> exitoso con la instancia creada, o fallido
    /// con el <see cref="Error"/> correspondiente si el valor no es válido.
    /// </returns>
    public static Result<ContractNumber> Create(string? value)
    {
        if (value is null)
        {
            return Result.Failure<ContractNumber>(
                new Error("ContractNumber.Null", "El número de contrato es obligatorio."));
        }

        var trimmedValue = value.Trim();

        if (trimmedValue.Length == 0)
        {
            return Result.Failure<ContractNumber>(
                new Error("ContractNumber.Empty", "El número de contrato no puede estar vacío."));
        }

        if (trimmedValue.Length > MaxLength)
        {
            return Result.Failure<ContractNumber>(
                new Error("ContractNumber.TooLong", $"El número de contrato no puede superar los {MaxLength} caracteres."));
        }

        if (!trimmedValue.StartsWith(Prefix, StringComparison.Ordinal))
        {
            return Result.Failure<ContractNumber>(
                new Error("ContractNumber.InvalidFormat", $"El número de contrato debe iniciar con \"{Prefix}\"."));
        }

        return Result.Success(new ContractNumber(trimmedValue));
    }
}
