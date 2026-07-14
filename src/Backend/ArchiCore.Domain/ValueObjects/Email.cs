using System.Net.Mail;
using ArchiCore.Domain.Common;

namespace ArchiCore.Domain.ValueObjects;

/// <summary>
/// Value Object que representa una dirección de correo electrónico.
/// </summary>
public sealed class Email : CodeValueObject
{
    private const int MaxLength = 254;

    private Email(string value)
        : base(value)
    {
    }

    /// <summary>
    /// Crea una instancia válida de <see cref="Email"/> a partir del valor especificado.
    /// </summary>
    /// <param name="value">Valor de la dirección de correo electrónico.</param>
    /// <returns>
    /// Un <see cref="Result{Email}"/> exitoso con la instancia creada, o fallido
    /// con el <see cref="Error"/> correspondiente si el valor no es válido.
    /// </returns>
    public static Result<Email> Create(string? value)
    {
        if (value is null)
        {
            return Result.Failure<Email>(
                new Error("Email.Null", "El correo electrónico es obligatorio."));
        }

        var trimmedValue = value.Trim();

        if (trimmedValue.Length == 0)
        {
            return Result.Failure<Email>(
                new Error("Email.Empty", "El correo electrónico no puede estar vacío."));
        }

        if (trimmedValue.Length > MaxLength)
        {
            return Result.Failure<Email>(
                new Error("Email.TooLong", $"El correo electrónico no puede superar los {MaxLength} caracteres."));
        }

        if (!IsValidFormat(trimmedValue))
        {
            return Result.Failure<Email>(
                new Error("Email.InvalidFormat", "El formato del correo electrónico no es válido."));
        }

        return Result.Success(new Email(trimmedValue));
    }

    private static bool IsValidFormat(string value)
    {
        try
        {
            var mailAddress = new MailAddress(value);
            return mailAddress.Address == value;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
