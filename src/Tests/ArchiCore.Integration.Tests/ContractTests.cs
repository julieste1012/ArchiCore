using System.Net;
using ArchiCore.Integration.Tests.Infrastructure;
using Xunit;

namespace ArchiCore.Integration.Tests;

[Collection(IntegrationCollection.Name)]
public sealed class ContractTests(ApiFixture fixture) : ApiTestBase(fixture)
{
    [Fact]
    public async Task Create_links_the_contract_to_its_customer_in_both_directions()
    {
        var customer = await CreateCustomerAsync();
        var number = TestData.ContractNumber();
        Assert.StartsWith("CTR-", number);

        var request = new
        {
            ContractNumber = number,
            ClientId = customer.Id,
            StartDate = FixedDate,
            TotalValue = 2_750_000.50m,
            Currency = "COP"
        };

        using var response = await PostAsync("/api/contracts", request);

        await AssertStatusAsync(HttpStatusCode.Created, response);
        var created = await ReadBodyAsync<ContractResponse>(response);
        Assert.EndsWith($"/api/contracts/{created.Id}", response.Headers.Location?.AbsolutePath);

        var contract = await GetJsonAsync<ContractResponse>($"/api/contracts/{created.Id}");
        Assert.Equal(created.Id, contract.Id);
        Assert.Equal(customer.Id, contract.ClientId);
        Assert.Equal(number, contract.ContractNumber);
        Assert.Equal("Draft", contract.Status);
        Assert.Equal(2_750_000.50m, contract.TotalValue);
        Assert.Equal("COP", contract.Currency);
        Assert.Equal(FixedDate.UtcDateTime, contract.StartDate.ToUniversalTime());
        Assert.Null(contract.EndDate);
        Assert.Empty(contract.Projects);

        var refreshedCustomer = await GetJsonAsync<CustomerResponse>($"/api/customers/{customer.Id}");
        var reference = Assert.Single(refreshedCustomer.Contracts, c => c.ContractId == created.Id);
        Assert.Equal(number, reference.ContractNumber);
    }

    [Fact]
    public async Task List_filtered_by_client_contains_the_created_contract()
    {
        var customer = await CreateCustomerAsync();
        var contract = await CreateContractAsync(customer.Id);

        var listed = await FindInListAsync<ContractResponse>(
            $"/api/contracts?clientId={customer.Id}", c => c.Id == contract.Id);

        Assert.NotNull(listed);
        Assert.Equal(customer.Id, listed.ClientId);
    }
}
