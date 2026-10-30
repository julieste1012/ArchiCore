using System.Net;
using ArchiCore.Domain.ValueObjects;
using ArchiCore.Integration.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ArchiCore.Integration.Tests;

[Collection(IntegrationCollection.Name)]
public sealed class WarehouseTests(ApiFixture fixture) : ApiTestBase(fixture)
{
    [Fact]
    public async Task Create_returns_201_and_the_warehouse_can_be_retrieved()
    {
        var code = TestData.WarehouseCode();
        Assert.StartsWith("WH-", code);

        using var response = await PostAsync("/api/warehouses", new { WarehouseCode = code });

        await AssertStatusAsync(HttpStatusCode.Created, response);
        var created = await ReadBodyAsync<WarehouseResponse>(response);
        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.EndsWith($"/api/warehouses/{created.Id}", response.Headers.Location?.AbsolutePath);

        var fetched = await GetJsonAsync<WarehouseResponse>($"/api/warehouses/{created.Id}");
        Assert.Equal(created.Id, fetched.Id);
        Assert.Equal(code, fetched.WarehouseCode);
        Assert.Equal("Available", fetched.Status);

        var listed = await FindInListAsync<WarehouseResponse>("/api/warehouses", w => w.Id == created.Id);
        Assert.NotNull(listed);
    }

    [Fact]
    public async Task Creating_a_warehouse_with_an_existing_code_returns_409_and_persists_nothing_new()
    {
        var first = await CreateWarehouseAsync();

        using var duplicate = await PostAsync("/api/warehouses", new { WarehouseCode = first.WarehouseCode });

        await AssertStatusAsync(HttpStatusCode.Conflict, duplicate);
        var problem = await ReadBodyAsync<ProblemResponse>(duplicate);
        Assert.Equal("Warehouse.DuplicateCode", problem.Code);

        var code = WarehouseCode.Create(first.WarehouseCode).Value;
        var rows = await Fixture.WithDbContextAsync(
            db => db.Warehouses.AsNoTracking().CountAsync(w => w.WarehouseCode == code, Ct));
        Assert.Equal(1, rows);
    }
}
