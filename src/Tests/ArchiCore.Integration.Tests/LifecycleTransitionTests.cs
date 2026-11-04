using System.Net;
using ArchiCore.Integration.Tests.Infrastructure;
using Xunit;

namespace ArchiCore.Integration.Tests;

[Collection(IntegrationCollection.Name)]
public sealed class LifecycleTransitionTests(ApiFixture fixture) : ApiTestBase(fixture)
{
    [Fact]
    public async Task Customer_follows_prospect_active_inactive_archived_lifecycle()
    {
        var customer = await CreateCustomerAsync();

        customer = await PostCustomerTransitionAsync(customer.Id, "activate", "Active");
        customer = await PostCustomerTransitionAsync(customer.Id, "deactivate", "Inactive");
        customer = await PostCustomerTransitionAsync(customer.Id, "archive", "Archived");

        var persisted = await GetJsonAsync<CustomerResponse>($"/api/customers/{customer.Id}");
        Assert.Equal("Archived", persisted.Status);
    }

    [Fact]
    public async Task Contract_follows_draft_active_suspended_active_finished_lifecycle()
    {
        var customer = await CreateCustomerAsync();
        var contract = await CreateContractAsync(customer.Id);

        contract = await PostContractTransitionAsync(contract.Id, "activate", "Active");
        contract = await PostContractTransitionAsync(contract.Id, "suspend", "Suspended");
        contract = await PostContractTransitionAsync(contract.Id, "activate", "Active");

        using var finish = await PostAsync(
            $"/api/contracts/{contract.Id}/finish",
            new { FinishDate = FixedDate.AddDays(1) });
        await AssertStatusAsync(HttpStatusCode.OK, finish);
        var finished = await ReadBodyAsync<ContractResponse>(finish);
        Assert.Equal("Finished", finished.Status);
        Assert.Equal(FixedDate.AddDays(1).UtcDateTime, finished.EndDate?.ToUniversalTime());

        var persisted = await GetJsonAsync<ContractResponse>($"/api/contracts/{contract.Id}");
        Assert.Equal("Finished", persisted.Status);
        Assert.NotNull(persisted.EndDate);
    }

    [Fact]
    public async Task Project_follows_documented_available_lifecycle_transitions()
    {
        var customer = await CreateCustomerAsync();
        var contract = await CreateContractAsync(customer.Id);
        var project = await CreateProjectAsync(contract.Id);

        project = await PostProjectTransitionAsync(project.Id, "plan", "Planned");
        project = await PostProjectTransitionAsync(project.Id, "start", "Active");
        project = await PostProjectTransitionAsync(project.Id, "suspend", "Suspended");
        project = await PostProjectTransitionAsync(project.Id, "resume", "Active");
        project = await PostProjectTransitionAsync(project.Id, "complete", "Completed");
        project = await PostProjectTransitionAsync(project.Id, "close", "Closed");
        project = await PostProjectTransitionAsync(project.Id, "archive", "Archived");

        var persisted = await GetJsonAsync<ProjectResponse>($"/api/projects/{project.Id}");
        Assert.Equal("Archived", persisted.Status);
    }

    [Fact]
    public async Task Warehouse_follows_available_operational_maintenance_closed_archived_lifecycle()
    {
        var warehouse = await CreateWarehouseAsync();

        warehouse = await PostWarehouseTransitionAsync(warehouse.Id, "activate", "Operational");
        warehouse = await PostWarehouseTransitionAsync(warehouse.Id, "maintenance/start", "Maintenance");
        warehouse = await PostWarehouseTransitionAsync(warehouse.Id, "maintenance/finish", "Operational");
        warehouse = await PostWarehouseTransitionAsync(warehouse.Id, "maintenance/start", "Maintenance");
        warehouse = await PostWarehouseTransitionAsync(warehouse.Id, "close", "Closed");
        warehouse = await PostWarehouseTransitionAsync(warehouse.Id, "archive", "Archived");

        var persisted = await GetJsonAsync<WarehouseResponse>($"/api/warehouses/{warehouse.Id}");
        Assert.Equal("Archived", persisted.Status);
    }

    private async Task<CustomerResponse> PostCustomerTransitionAsync(Guid id, string transition, string expectedStatus)
    {
        using var response = await PostAsync($"/api/customers/{id}/{transition}", new { });
        await AssertStatusAsync(HttpStatusCode.OK, response);
        var result = await ReadBodyAsync<CustomerResponse>(response);
        Assert.Equal(expectedStatus, result.Status);
        return result;
    }

    private async Task<ContractResponse> PostContractTransitionAsync(Guid id, string transition, string expectedStatus)
    {
        using var response = await PostAsync($"/api/contracts/{id}/{transition}", new { });
        await AssertStatusAsync(HttpStatusCode.OK, response);
        var result = await ReadBodyAsync<ContractResponse>(response);
        Assert.Equal(expectedStatus, result.Status);
        return result;
    }

    private async Task<ProjectResponse> PostProjectTransitionAsync(Guid id, string transition, string expectedStatus)
    {
        using var response = await PostAsync($"/api/projects/{id}/{transition}", new { });
        await AssertStatusAsync(HttpStatusCode.OK, response);
        var result = await ReadBodyAsync<ProjectResponse>(response);
        Assert.Equal(expectedStatus, result.Status);
        return result;
    }

    private async Task<WarehouseResponse> PostWarehouseTransitionAsync(Guid id, string transition, string expectedStatus)
    {
        using var response = await PostAsync($"/api/warehouses/{id}/{transition}", new { });
        await AssertStatusAsync(HttpStatusCode.OK, response);
        var result = await ReadBodyAsync<WarehouseResponse>(response);
        Assert.Equal(expectedStatus, result.Status);
        return result;
    }
}
