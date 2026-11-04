using System.Net;
using ArchiCore.Integration.Tests.Infrastructure;
using Xunit;

namespace ArchiCore.Integration.Tests;

[Collection(IntegrationCollection.Name)]
public sealed class ProjectWarehouseAssignmentTests(ApiFixture fixture) : ApiTestBase(fixture)
{
    [Fact]
    public async Task Assigned_warehouse_is_active_and_persists_when_the_project_is_queried_again()
    {
        var warehouse = await CreateWarehouseAsync();
        var customer = await CreateCustomerAsync();
        var contract = await CreateContractAsync(customer.Id);
        var project = await CreateProjectAsync(contract.Id);
        var assignedFrom = new DateTimeOffset(2026, 3, 1, 8, 0, 0, TimeSpan.Zero);

        using var response = await PostAsync(
            $"/api/projects/{project.Id}/warehouses",
            new { WarehouseId = warehouse.Id, AssignedFrom = assignedFrom });

        await AssertStatusAsync(HttpStatusCode.OK, response);
        var assigned = await ReadBodyAsync<ProjectResponse>(response);
        AssertActiveAssignment(assigned, warehouse.Id, assignedFrom);

        var reloaded = await GetJsonAsync<ProjectResponse>($"/api/projects/{project.Id}");
        Assert.Equal(project.Id, reloaded.Id);
        AssertActiveAssignment(reloaded, warehouse.Id, assignedFrom);
    }

    [Fact]
    public async Task Assigning_the_same_warehouse_twice_returns_409_and_keeps_a_single_assignment()
    {
        var warehouse = await CreateWarehouseAsync();
        var customer = await CreateCustomerAsync();
        var contract = await CreateContractAsync(customer.Id);
        var project = await CreateProjectAsync(contract.Id);
        var request = new { WarehouseId = warehouse.Id, AssignedFrom = FixedDate };

        using var first = await PostAsync($"/api/projects/{project.Id}/warehouses", request);
        await AssertStatusAsync(HttpStatusCode.OK, first);

        using var second = await PostAsync($"/api/projects/{project.Id}/warehouses", request);

        await AssertStatusAsync(HttpStatusCode.Conflict, second);
        var problem = await ReadBodyAsync<ProblemResponse>(second);
        Assert.Equal("Project.DuplicateWarehouseAssignment", problem.Code);

        var reloaded = await GetJsonAsync<ProjectResponse>($"/api/projects/{project.Id}");
        Assert.Single(reloaded.WarehouseAssignments);
    }

    private static void AssertActiveAssignment(ProjectResponse project, Guid warehouseId, DateTimeOffset assignedFrom)
    {
        var assignment = Assert.Single(project.WarehouseAssignments);
        Assert.Equal(warehouseId, assignment.WarehouseId);
        Assert.True(assignment.IsActive);
        Assert.Null(assignment.ReleasedOn);
        Assert.Equal(assignedFrom.UtcDateTime, assignment.AssignedFrom.ToUniversalTime());
    }
}
