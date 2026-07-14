using ArchiCore.Domain.Common;

namespace ArchiCore.Domain.ValueObjects;

/// <summary>
/// Value Object que representa el código identificador de una carpeta,
/// con formato "FLD-000001".
/// </summary>
public sealed class FolderCode : CodeValueObject
{
    private const int MaxLength = 20;
    private const string Prefix = "FLD-";

    private FolderCode(string value)
        : base(value)
    {
    }

    /// <summary>
    /// Crea una instancia válida de <see cref="FolderCode"/> a partir del valor especificado.
    /// </summary>
    /// <param name="value">Valor del código de carpeta.</param>
    /// <returns>
    /// Un <see cref="Result{FolderCode}"/> exitoso con la instancia creada, o fallido
    /// con el <see cref="Error"/> correspondiente si el valor no es válido.
    /// </returns>
    public static Result<FolderCode> Create(string? value)
    {
        if (value is null)
        {
            return Result.Failure<FolderCode>(
                new Error("FolderCode.Null", "El código de carpeta es obligatorio."));
        }

        var trimmedValue = value.Trim();

        if (trimmedValue.Length == 0)
        {
            return Result.Failure<FolderCode>(
                new Error("FolderCode.Empty", "El código de carpeta no puede estar vacío."));
        }

        if (trimmedValue.Length > MaxLength)
        {
            return Result.Failure<FolderCode>(
                new Error("FolderCode.TooLong", $"El código de carpeta no puede superar los {MaxLength} caracteres."));
        }

        if (!trimmedValue.StartsWith(Prefix, StringComparison.Ordinal))
        {
            return Result.Failure<FolderCode>(
                new Error("FolderCode.InvalidFormat", $"El código de carpeta debe iniciar con \"{Prefix}\"."));
        }

        return Result.Success(new FolderCode(trimmedValue));
    }
}
