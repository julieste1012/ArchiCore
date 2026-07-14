namespace ArchiCore.Domain.ValueObjects;

using ArchiCore.Domain.Common;

/// <summary>
/// Value Object que representa un valor monetario, compuesto por un monto y una moneda.
/// </summary>
public sealed class Money : ValueObject
{
    private const int CurrencyLength = 3;

    /// <summary>
    /// Monto del valor monetario.
    /// </summary>
    public decimal Amount { get; }

    /// <summary>
    /// Código de la moneda, almacenado en mayúsculas (ej. "COP", "USD").
    /// </summary>
    public string Currency { get; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    /// <summary>
    /// Crea una instancia válida de <see cref="Money"/> a partir del monto y la moneda especificados.
    /// </summary>
    /// <param name="amount">Monto del valor monetario.</param>
    /// <param name="currency">Código de la moneda.</param>
    /// <returns>
    /// Un <see cref="Result{Money}"/> exitoso con la instancia creada, o fallido
    /// con el <see cref="Error"/> correspondiente si los valores no son válidos.
    /// </returns>
    public static Result<Money> Create(decimal amount, string? currency)
    {
        if (amount < 0)
        {
            return Result.Failure<Money>(
                new Error("Money.NegativeAmount", "El monto no puede ser negativo."));
        }

        if (decimal.Round(amount, 2) != amount)
        {
            return Result.Failure<Money>(
                new Error("Money.InvalidPrecision", "El monto no puede tener más de dos decimales."));
        }

        if (currency is null)
        {
            return Result.Failure<Money>(
                new Error("Money.CurrencyNull", "La moneda es obligatoria."));
        }

        var trimmedCurrency = currency.Trim();

        if (trimmedCurrency.Length == 0)
        {
            return Result.Failure<Money>(
                new Error("Money.CurrencyEmpty", "La moneda no puede estar vacía."));
        }

        if (trimmedCurrency.Length != CurrencyLength)
        {
            return Result.Failure<Money>(
                new Error("Money.InvalidCurrencyLength", $"La moneda debe tener exactamente {CurrencyLength} caracteres."));
        }

        return Result.Success(new Money(amount, trimmedCurrency.ToUpperInvariant()));
    }

    /// <inheritdoc />
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    /// <inheritdoc />
    public override string ToString() => $"{Amount:0.00} {Currency}";
}
