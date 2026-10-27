using ArchiCore.Domain.Common;
using Xunit;

namespace ArchiCore.Domain.Tests.Common;

public sealed class ResultTests
{
    [Fact]
    public void Success_without_value_is_successful_and_has_no_error()
    {
        var result = Result.Success();

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(Error.None, result.Error);
    }

    [Fact]
    public void Failure_preserves_the_error()
    {
        var error = new Error("Customer.Invalid", "The customer is invalid.");

        var result = Result.Failure(error);

        Assert.True(result.IsFailure);
        Assert.False(result.IsSuccess);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void Generic_success_exposes_its_value()
    {
        var result = Result.Success(42);

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void Generic_failure_does_not_expose_a_value()
    {
        var result = Result.Failure<string>(new Error("Test.Failure", "Expected failure."));

        Assert.Throws<InvalidOperationException>(() => _ = result.Value);
    }

    [Fact]
    public void Errors_with_the_same_code_and_message_are_equal()
    {
        var first = new Error("Test.Invalid", "Invalid value.");
        var second = new Error("Test.Invalid", "Invalid value.");

        Assert.Equal(first, second);
    }
}
