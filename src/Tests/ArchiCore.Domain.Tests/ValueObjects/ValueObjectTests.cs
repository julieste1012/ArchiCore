using ArchiCore.Domain.ValueObjects;
using Xunit;

namespace ArchiCore.Domain.Tests.ValueObjects;

public sealed class ValueObjectTests
{
    [Fact]
    public void Nit_trims_surrounding_whitespace()
    {
        var result = Nit.Create(" 900123456-1 ");

        Assert.True(result.IsSuccess);
        Assert.Equal("900123456-1", result.Value.Value);
    }

    [Fact]
    public void Nit_rejects_null()
    {
        var result = Nit.Create(null);

        Assert.True(result.IsFailure);
        Assert.Equal("Nit.Null", result.Error.Code);
    }

    [Fact]
    public void Contract_number_requires_the_documented_prefix()
    {
        var result = ContractNumber.Create("CON-2026-000001");

        Assert.True(result.IsFailure);
        Assert.Equal("ContractNumber.InvalidFormat", result.Error.Code);
    }

    [Fact]
    public void Equal_codes_have_structural_equality()
    {
        var first = ContractNumber.Create("CTR-2026-000001").Value;
        var second = ContractNumber.Create("CTR-2026-000001").Value;

        Assert.Equal(first, second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
    }

    [Fact]
    public void Money_normalizes_currency_to_uppercase()
    {
        var result = Money.Create(1200.50m, "cop");

        Assert.True(result.IsSuccess);
        Assert.Equal("COP", result.Value.Currency);
    }

    [Fact]
    public void Money_rejects_negative_amounts()
    {
        var result = Money.Create(-0.01m, "COP");

        Assert.True(result.IsFailure);
        Assert.Equal("Money.NegativeAmount", result.Error.Code);
    }

    [Fact]
    public void Money_rejects_more_than_two_decimal_places()
    {
        var result = Money.Create(10.123m, "COP");

        Assert.True(result.IsFailure);
        Assert.Equal("Money.InvalidPrecision", result.Error.Code);
    }
}
