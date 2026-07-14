using ArchiCore.Domain.Common;

namespace ArchiCore.Domain.ValueObjects;

/// <summary>
/// Value Object que representa el código identificador de un proyecto,
/// con formato "PRJ-2026-000001".
/// </summary>
public sealed class ProjectCode : CodeValueObject
{
    private const int MaxLength = 20;
    private const string Prefix = "PRJ-";

    private ProjectCode(string value)
        : base(value)
    {
    }

    /// <summary>
    /// Crea una instancia válida de <see cref="ProjectCode"/> a partir del valor especificado.
    /// </summary>
    /// <param name="value">Valor del código de proyecto.</param>
    /// <returns>
    /// Un <see cref="Result{ProjectCode}"/> exitoso con la instancia creada, o fallido
    /// con el <see cref="Error"/> correspondiente si el valor no es válido.
    /// </returns>
    public static Result<ProjectCode> Create(string? value)
    {
        if (value is null)
        {
            return Result.Failure<ProjectCode>(
                new Error("ProjectCode.Null", "El código de proyecto es obligatorio."));
        }

        var trimmedValue = value.Trim();

        if (trimmedValue.Length == 0)
        {
            return Result.Failure<ProjectCode>(
                new Error("ProjectCode.Empty", "El código de proyecto no puede estar vacío."));
        }

        if (trimmedValue.Length > MaxLength)
        {
            return Result.Failure<ProjectCode>(
                new Error("ProjectCode.TooLong", $"El código de proyecto no puede superar los {MaxLength} caracteres."));
        }

        if (!trimmedValue.StartsWith(Prefix, StringComparison.Ordinal))
        {
            return Result.Failure<ProjectCode>(
                new Error("ProjectCode.InvalidFormat", $"El código de proyecto debe iniciar con \"{Prefix}\"."));
        }

        return Result.Success(new ProjectCode(trimmedValue));
    }
}
