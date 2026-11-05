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


    [Fact]
    public async Task Archive_classification_and_folder_creation_are_available_through_the_api()
    {
        var customer = await CreateCustomerAsync();
        var contract = await CreateContractAsync(customer.Id);
        var project = await CreateProjectAsync(contract.Id);

        using var archiveResponse = await PostAsync(
            $"/api/projects/{project.Id}/documentary-archive",
            new { });
        await AssertStatusAsync(HttpStatusCode.Created, archiveResponse);
        var archive = await ReadBodyAsync<ArchiveSummaryResponse>(archiveResponse);

        var sectionId = await PostIdAsync(
            $"/api/archives/{archive.Id}/sections",
            new { Code = "SEC-001", Name = "Gestión administrativa" });
        var seriesId = await PostIdAsync(
            $"/api/archives/{archive.Id}/sections/{sectionId}/series",
            new { Code = "SER-001", Name = "Contratos" });
        var subseriesId = await PostIdAsync(
            $"/api/archives/{archive.Id}/series/{seriesId}/subseries",
            new { Code = "SUB-001", Name = "Contratos de servicios" });
        var expedientId = await PostIdAsync(
            $"/api/archives/{archive.Id}/subseries/{subseriesId}/expedients",
            new { Reference = "EXP-" + Guid.NewGuid().ToString("N")[..8], Title = "Contrato de servicios 2026" });

        var folderCode = "FLD-" + Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        using var folderResponse = await PostAsync(
            $"/api/archives/{archive.Id}/folders",
            new { FolderCode = folderCode, ExpedientId = expedientId });
        await AssertStatusAsync(HttpStatusCode.Created, folderResponse);
        var createdFolder = await ReadBodyAsync<FolderCreatedResponse>(folderResponse);

        Assert.NotEqual(Guid.Empty, createdFolder.Id);
        Assert.Equal(archive.Id, createdFolder.ArchiveId);

        using var listResponse = await GetAsync($"/api/archives/{archive.Id}/folders?page=1&pageSize=100");
        await AssertStatusAsync(HttpStatusCode.OK, listResponse);
    }

    private async Task<Guid> PostIdAsync(string path, object request)
    {
        using var response = await PostAsync(path, request);
        await AssertStatusAsync(HttpStatusCode.OK, response);
        return await ReadBodyAsync<Guid>(response);
    }

    private sealed record FolderCreatedResponse(Guid Id, Guid ArchiveId);

    private sealed record ArchiveSummaryResponse(Guid Id, Guid ProjectId);
}
