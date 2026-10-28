using ArchiCore.Domain.ValueObjects;
using ArchiCore.Integration.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ArchiCore.Integration.Tests;

[Collection(IntegrationCollection.Name)]
public sealed class IsolationAndWiringTests(ApiFixture fixture) : ApiTestBase(fixture)
{
    [Fact]
    public async Task Api_uses_the_temporary_container_database_and_not_the_developer_configuration()
    {
        var apiConnectionString = await Fixture.WithDbContextAsync(
            db => Task.FromResult(db.Database.GetConnectionString()));

        Assert.Equal(Fixture.ConnectionString, apiConnectionString);
        Assert.Contains($"Database={ApiFixture.DatabaseName}", apiConnectionString, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Existing_migrations_were_applied_to_the_temporary_database()
    {
        var applied = await Fixture.WithDbContextAsync(
            async db => (await db.Database.GetAppliedMigrationsAsync(Ct)).ToList());
        var pending = await Fixture.WithDbContextAsync(
            async db => (await db.Database.GetPendingMigrationsAsync(Ct)).ToList());

        Assert.Contains(applied, id => id.EndsWith("_InitialCoreSchema", StringComparison.Ordinal));
        Assert.Empty(pending);
    }

    [Fact]
    public void Generated_identifiers_are_unique_and_satisfy_the_value_object_rules()
    {
        const int count = 1_000;
        var nits = new HashSet<string>();
        var contractNumbers = new HashSet<string>();
        var projectCodes = new HashSet<string>();
        var warehouseCodes = new HashSet<string>();

        for (var i = 0; i < count; i++)
        {
            var nit = TestData.Nit();
            var contractNumber = TestData.ContractNumber();
            var projectCode = TestData.ProjectCode();
            var warehouseCode = TestData.WarehouseCode();

            Assert.True(Nit.Create(nit).IsSuccess, $"Invalid NIT: {nit}");
            Assert.True(ContractNumber.Create(contractNumber).IsSuccess, $"Invalid contract number: {contractNumber}");
            Assert.True(ProjectCode.Create(projectCode).IsSuccess, $"Invalid project code: {projectCode}");
            Assert.True(WarehouseCode.Create(warehouseCode).IsSuccess, $"Invalid warehouse code: {warehouseCode}");

            nits.Add(nit);
            contractNumbers.Add(contractNumber);
            projectCodes.Add(projectCode);
            warehouseCodes.Add(warehouseCode);
        }

        Assert.Equal(count, nits.Count);
        Assert.Equal(count, contractNumbers.Count);
        Assert.Equal(count, projectCodes.Count);
        Assert.Equal(count, warehouseCodes.Count);
    }

    [Fact]
    public async Task Independent_flows_running_concurrently_do_not_collide_and_each_one_is_verified_by_id()
    {
        var flows = await Task.WhenAll(Enumerable.Range(0, 5).Select(_ => RunFullFlowAsync()));

        Assert.Equal(flows.Length, flows.Select(f => f.CustomerId).Distinct().Count());
        Assert.Equal(flows.Length, flows.Select(f => f.ProjectId).Distinct().Count());

        foreach (var flow in flows)
        {
            var customer = await GetJsonAsync<CustomerResponse>($"/api/customers/{flow.CustomerId}");
            Assert.Contains(customer.Contracts, c => c.ContractId == flow.ContractId);

            var contract = await GetJsonAsync<ContractResponse>($"/api/contracts/{flow.ContractId}");
            Assert.Equal(flow.CustomerId, contract.ClientId);
            Assert.Contains(contract.Projects, p => p.ProjectId == flow.ProjectId);

            var project = await GetJsonAsync<ProjectResponse>($"/api/projects/{flow.ProjectId}");
            Assert.Equal(flow.ContractId, project.ContractId);
            var assignment = Assert.Single(project.WarehouseAssignments);
            Assert.Equal(flow.WarehouseId, assignment.WarehouseId);
            Assert.True(assignment.IsActive);
        }
    }

    private async Task<(Guid CustomerId, Guid ContractId, Guid ProjectId, Guid WarehouseId)> RunFullFlowAsync()
    {
        var customer = await CreateCustomerAsync();
        var warehouse = await CreateWarehouseAsync();
        var contract = await CreateContractAsync(customer.Id);
        var project = await CreateProjectAsync(contract.Id);

        using var response = await PostAsync(
            $"/api/projects/{project.Id}/warehouses",
            new { WarehouseId = warehouse.Id, AssignedFrom = FixedDate });
        await AssertStatusAsync(System.Net.HttpStatusCode.OK, response);

        return (customer.Id, contract.Id, project.Id, warehouse.Id);
    }
}
