using System.Net;
using ArchiCore.Integration.Tests.Infrastructure;
using Xunit;

namespace ArchiCore.Integration.Tests;

[Collection(IntegrationCollection.Name)]
public sealed class DocumentaryArchiveApiTests(ApiFixture fixture) : ApiTestBase(fixture)
{
    [Fact]
    public async Task Documentary_archive_route_is_distinct_from_project_archive_lifecycle()
    {
        var customer = await CreateCustomerAsync();
        var contract = await CreateContractAsync(customer.Id);
        var project = await CreateProjectAsync(contract.Id);

        using var createResponse = await PostAsync(
            $"/api/projects/{project.Id}/documentary-archive",
            new { });
        await AssertStatusAsync(HttpStatusCode.Created, createResponse);
        var created = await ReadBodyAsync<ArchiveSummaryResponse>(createResponse);

        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.Equal(project.Id, created.ProjectId);

        var persisted = await GetJsonAsync<ArchiveSummaryResponse>(
            $"/api/projects/{project.Id}/documentary-archive");
        Assert.Equal(created.Id, persisted.Id);
        Assert.Equal(project.Id, persisted.ProjectId);

        using var duplicateResponse = await PostAsync(
            $"/api/projects/{project.Id}/documentary-archive",
            new { });
        await AssertStatusAsync(HttpStatusCode.Conflict, duplicateResponse);
    }

    private sealed record ArchiveSummaryResponse(Guid Id, Guid ProjectId);
}
