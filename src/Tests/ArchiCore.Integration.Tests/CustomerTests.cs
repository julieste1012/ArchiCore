using System.Net;
using ArchiCore.Domain.ValueObjects;
using ArchiCore.Integration.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ArchiCore.Integration.Tests;

[Collection(IntegrationCollection.Name)]
public sealed class CustomerTests(ApiFixture fixture) : ApiTestBase(fixture)
{
    [Fact]
    public async Task Create_returns_201_with_location_of_the_new_customer()
    {
        var nit = TestData.Nit();

        using var response = await PostAsync("/api/customers", new { Nit = nit });

        await AssertStatusAsync(HttpStatusCode.Created, response);
        var created = await ReadBodyAsync<CustomerResponse>(response);
        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.Equal(nit, created.Nit);
        Assert.EndsWith($"/api/customers/{created.Id}", response.Headers.Location?.AbsolutePath);
    }

    [Fact]
    public async Task Get_by_id_returns_the_persisted_customer()
    {
        var created = await CreateCustomerAsync();

        var fetched = await GetJsonAsync<CustomerResponse>($"/api/customers/{created.Id}");

        Assert.Equal(created.Id, fetched.Id);
        Assert.Equal(created.Nit, fetched.Nit);
        Assert.Equal("Prospect", fetched.Status);
        Assert.Empty(fetched.Contracts);
    }

    [Fact]
    public async Task List_contains_the_created_customer()
    {
        var created = await CreateCustomerAsync();

        var listed = await FindInListAsync<CustomerResponse>("/api/customers", c => c.Id == created.Id);

        Assert.NotNull(listed);
        Assert.Equal(created.Nit, listed.Nit);
    }

    [Fact]
    public async Task Creating_a_customer_with_an_existing_nit_returns_409_and_persists_nothing_new()
    {
        var first = await CreateCustomerAsync();

        using var duplicate = await PostAsync("/api/customers", new { Nit = first.Nit });

        await AssertStatusAsync(HttpStatusCode.Conflict, duplicate);
        var problem = await ReadBodyAsync<ProblemResponse>(duplicate);
        Assert.Equal("Customer.DuplicateNit", problem.Code);

        var nit = Nit.Create(first.Nit).Value;
        var rows = await Fixture.WithDbContextAsync(
            db => db.Customers.AsNoTracking().CountAsync(c => c.Nit == nit, Ct));
        Assert.Equal(1, rows);
    }
}
