using System.Net;
using ArchiCore.Integration.Tests.Infrastructure;
using Xunit;

namespace ArchiCore.Integration.Tests;

[Collection(IntegrationCollection.Name)]
public sealed class ProjectTests(ApiFixture fixture) : ApiTestBase(fixture)
{
    [Fact]
    public async Task Create_links_the_project_to_its_contract_in_both_directions()
    {
        var customer = await CreateCustomerAsync();
        var contract = await CreateContractAsync(customer.Id);
        var code = TestData.ProjectCode();
        Assert.StartsWith("PRJ-", code);

        using var response = await PostAsync("/api/projects", new { ProjectCode = code, ContractId = contract.Id });

        await AssertStatusAsync(HttpStatusCode.Created, response);
        var created = await ReadBodyAsync<ProjectResponse>(response);
        Assert.EndsWith($"/api/projects/{created.Id}", response.Headers.Location?.AbsolutePath);

        var project = await GetJsonAsync<ProjectResponse>($"/api/projects/{created.Id}");
        Assert.Equal(created.Id, project.Id);
        Assert.Equal(contract.Id, project.ContractId);
        Assert.Equal(code, project.ProjectCode);
        Assert.Equal("Draft", project.Status);
        Assert.Empty(project.WarehouseAssignments);

        var refreshedContract = await GetJsonAsync<ContractResponse>($"/api/contracts/{contract.Id}");
        var reference = Assert.Single(refreshedContract.Projects, p => p.ProjectId == created.Id);
        Assert.Equal(code, reference.ProjectCode);
    }

    [Fact]
    public async Task List_filtered_by_contract_contains_the_created_project()
    {
        var customer = await CreateCustomerAsync();
        var contract = await CreateContractAsync(customer.Id);
        var project = await CreateProjectAsync(contract.Id);

        var listed = await FindInListAsync<ProjectResponse>(
            $"/api/projects?contractId={contract.Id}", p => p.Id == project.Id);

        Assert.NotNull(listed);
        Assert.Equal(contract.Id, listed.ContractId);
    }
}
