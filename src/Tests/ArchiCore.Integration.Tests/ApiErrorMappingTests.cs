using System.Net;
using ArchiCore.Integration.Tests.Infrastructure;
using Xunit;

namespace ArchiCore.Integration.Tests;

[Collection(IntegrationCollection.Name)]
public sealed class ApiErrorMappingTests(ApiFixture fixture) : ApiTestBase(fixture)
{
    [Fact]
    public async Task Missing_customer_returns_404_with_stable_error_code()
    {
        using var response = await GetAsync($"/api/customers/{Guid.NewGuid()}");

        await AssertStatusAsync(HttpStatusCode.NotFound, response);
        var problem = await ReadBodyAsync<ProblemResponse>(response);
        Assert.Equal("Customer.NotFound", problem.Code);
        Assert.Equal(404, problem.Status);
    }

    [Fact]
    public async Task Invalid_customer_nit_returns_422_with_validation_error_code()
    {
        using var response = await PostAsync("/api/customers", new { Nit = "" });

        await AssertStatusAsync(HttpStatusCode.UnprocessableEntity, response);
        var problem = await ReadBodyAsync<ProblemResponse>(response);
        Assert.Equal("Nit.Empty", problem.Code);
        Assert.Equal(422, problem.Status);
    }

    [Fact]
    public async Task Contract_for_missing_customer_returns_404()
    {
        using var response = await PostAsync("/api/contracts", new
        {
            ContractNumber = TestData.ContractNumber(),
            ClientId = Guid.NewGuid(),
            StartDate = FixedDate,
            TotalValue = 250_000m,
            Currency = "COP"
        });

        await AssertStatusAsync(HttpStatusCode.NotFound, response);
        var problem = await ReadBodyAsync<ProblemResponse>(response);
        Assert.Equal("Contract.CustomerNotFound", problem.Code);
    }

    [Fact]
    public async Task Invalid_customer_state_transition_returns_409_and_does_not_change_state()
    {
        var customer = await CreateCustomerAsync();

        using var invalidTransition = await PostAsync($"/api/customers/{customer.Id}/deactivate", new { });

        await AssertStatusAsync(HttpStatusCode.Conflict, invalidTransition);
        var problem = await ReadBodyAsync<ProblemResponse>(invalidTransition);
        Assert.Equal("Customer.InvalidDeactivation", problem.Code);

        var persisted = await GetJsonAsync<CustomerResponse>($"/api/customers/{customer.Id}");
        Assert.Equal("Prospect", persisted.Status);
    }
}
