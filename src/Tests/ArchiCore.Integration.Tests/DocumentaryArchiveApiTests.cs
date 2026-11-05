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

        using var receiveResponse = await PostAsync($"/api/folders/{createdFolder.Id}/receive", new { });
        await AssertStatusAsync(HttpStatusCode.NoContent, receiveResponse);
        using var organizeResponse = await PostAsync($"/api/folders/{createdFolder.Id}/organize", new { });
        await AssertStatusAsync(HttpStatusCode.NoContent, organizeResponse);

        var documentId = await PostIdAsync(
            $"/api/folders/{createdFolder.Id}/documents",
            new { Title = "Contrato firmado", DocumentDate = (DateTime?)null });

        using var folioResponse = await PostAsync(
            $"/api/folders/{createdFolder.Id}/documents/{documentId}/folios",
            new { Number = 1 });
        await AssertStatusAsync(HttpStatusCode.OK, folioResponse);
        Assert.Equal(1, await ReadBodyAsync<int>(folioResponse));

        using var annexResponse = await PostAsync(
            $"/api/folders/{createdFolder.Id}/documents/{documentId}/annexes",
            new { Title = "Anexo técnico" });
        await AssertStatusAsync(HttpStatusCode.OK, annexResponse);
        Assert.Equal(1, await ReadBodyAsync<int>(annexResponse));

        using var interleavedFolioResponse = await PostAsync(
            $"/api/folders/{createdFolder.Id}/documents/{documentId}/folios",
            new { Number = 2 });
        await AssertStatusAsync(HttpStatusCode.Conflict, interleavedFolioResponse);

        var folderDetail = await GetJsonAsync<FolderDetailResponse>($"/api/folders/{createdFolder.Id}");
        Assert.Equal("Organized", folderDetail.Status);
        var document = Assert.Single(folderDetail.Documents);
        Assert.Equal(new[] { 1 }, document.Folios);
        Assert.Equal("Anexo técnico", Assert.Single(document.Annexes).Title);
    }


    [Fact]
    public async Task Box_is_stored_in_a_position_and_warehouse_occupancy_is_consistent()
    {
        var warehouse = await CreateWarehouseAsync();

        using var activationResponse = await PostAsync($"/api/warehouses/{warehouse.Id}/activate", new { });
        await AssertStatusAsync(HttpStatusCode.OK, activationResponse);

        var aisleId = await PostIdAsync(
            $"/api/warehouses/{warehouse.Id}/locations",
            new { Code = "AIS-" + Guid.NewGuid().ToString("N")[..6], Type = "Aisle", ParentLocationId = (Guid?)null, Capacity = (int?)null });
        var shelfId = await PostIdAsync(
            $"/api/warehouses/{warehouse.Id}/locations",
            new { Code = "SH-" + Guid.NewGuid().ToString("N")[..6], Type = "Shelf", ParentLocationId = aisleId, Capacity = (int?)null });
        var levelId = await PostIdAsync(
            $"/api/warehouses/{warehouse.Id}/locations",
            new { Code = "LV-" + Guid.NewGuid().ToString("N")[..6], Type = "Level", ParentLocationId = shelfId, Capacity = (int?)null });
        var positionId = await PostIdAsync(
            $"/api/warehouses/{warehouse.Id}/locations",
            new { Code = "POS-" + Guid.NewGuid().ToString("N")[..6], Type = "Position", ParentLocationId = levelId, Capacity = 2 });

        var boxCode = "BOX-" + Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        using var boxResponse = await PostAsync("/api/boxes", new { BoxCode = boxCode });
        await AssertStatusAsync(HttpStatusCode.Created, boxResponse);
        var createdBox = await ReadBodyAsync<BoxCreatedResponse>(boxResponse);

        var barcode = "BC-" + Guid.NewGuid().ToString("N");
        using var barcodeResponse = await PostAsync(
            $"/api/boxes/{createdBox.Id}/barcode",
            new { Barcode = barcode });
        await AssertStatusAsync(HttpStatusCode.NoContent, barcodeResponse);

        using var availableResponse = await PostAsync($"/api/boxes/{createdBox.Id}/available", new { });
        await AssertStatusAsync(HttpStatusCode.NoContent, availableResponse);

        using var storeResponse = await PostAsync(
            $"/api/boxes/{createdBox.Id}/store",
            new { WarehouseId = warehouse.Id, LocationId = positionId });
        await AssertStatusAsync(HttpStatusCode.NoContent, storeResponse);

        using var boxDetailResponse = await GetAsync($"/api/boxes/{createdBox.Id}");
        await AssertStatusAsync(HttpStatusCode.OK, boxDetailResponse);
        var boxDetail = await ReadBodyAsync<BoxDetailResponse>(boxDetailResponse);
        Assert.Equal("InStorage", boxDetail.Status);
        Assert.Equal(positionId, boxDetail.WarehouseLocationId);

        var physicalLocations = await GetJsonAsync<List<WarehouseLocationResponse>>(
            $"/api/warehouses/{warehouse.Id}/locations");
        var storedPosition = Assert.Single(physicalLocations, item => item.Id == positionId);
        Assert.Equal(1, storedPosition.OccupiedCapacity);
        Assert.Contains(createdBox.Id, storedPosition.BoxIds);

        var secondPositionId = await PostIdAsync(
            $"/api/warehouses/{warehouse.Id}/locations",
            new { Code = "POS-" + Guid.NewGuid().ToString("N")[..6], Type = "Position", ParentLocationId = levelId, Capacity = 2 });

        using var relocateResponse = await PostAsync(
            $"/api/boxes/{createdBox.Id}/relocate",
            new
            {
                SourceWarehouseId = warehouse.Id,
                DestinationWarehouseId = warehouse.Id,
                DestinationLocationId = secondPositionId
            });
        await AssertStatusAsync(HttpStatusCode.NoContent, relocateResponse);

        var afterRelocation = await GetJsonAsync<List<WarehouseLocationResponse>>(
            $"/api/warehouses/{warehouse.Id}/locations");
        var releasedPosition = Assert.Single(afterRelocation, item => item.Id == positionId);
        var destinationPosition = Assert.Single(afterRelocation, item => item.Id == secondPositionId);
        Assert.Equal(0, releasedPosition.OccupiedCapacity);
        Assert.Empty(releasedPosition.BoxIds);
        Assert.Equal(1, destinationPosition.OccupiedCapacity);
        Assert.Contains(createdBox.Id, destinationPosition.BoxIds);

        using var moveOutResponse = await PostAsync(
            $"/api/boxes/{createdBox.Id}/move-out?warehouseId={warehouse.Id}",
            new { });
        await AssertStatusAsync(HttpStatusCode.NoContent, moveOutResponse);

        var afterMoveOut = await GetJsonAsync<List<WarehouseLocationResponse>>(
            $"/api/warehouses/{warehouse.Id}/locations");
        var finalPosition = Assert.Single(afterMoveOut, item => item.Id == secondPositionId);
        Assert.Equal(0, finalPosition.OccupiedCapacity);
        Assert.Empty(finalPosition.BoxIds);

        using var finalBoxResponse = await GetAsync($"/api/boxes/{createdBox.Id}");
        await AssertStatusAsync(HttpStatusCode.OK, finalBoxResponse);
        var finalBox = await ReadBodyAsync<BoxDetailResponse>(finalBoxResponse);
        Assert.Equal("InTransit", finalBox.Status);
        Assert.Null(finalBox.WarehouseLocationId);
    }

    private sealed record BoxCreatedResponse(Guid Id);
    private sealed record BoxDetailResponse(Guid Id, string Status, Guid? WarehouseLocationId);
    private sealed record WarehouseLocationResponse(Guid Id, string Code, string Type, int? Capacity, int OccupiedCapacity, IReadOnlyList<Guid> BoxIds);

    private async Task<Guid> PostIdAsync(string path, object request)
    {
        using var response = await PostAsync(path, request);
        await AssertStatusAsync(HttpStatusCode.OK, response);
        return await ReadBodyAsync<Guid>(response);
    }

    private sealed record FolderCreatedResponse(Guid Id, Guid ArchiveId);
    private sealed record FolderDetailResponse(Guid Id, string Status, IReadOnlyList<FolderDocumentDetailResponse> Documents);
    private sealed record FolderDocumentDetailResponse(Guid Id, string Title, IReadOnlyList<int> Folios, IReadOnlyList<FolderAnnexDetailResponse> Annexes);
    private sealed record FolderAnnexDetailResponse(string Title, int Sequence);

    private sealed record ArchiveSummaryResponse(Guid Id, Guid ProjectId);
}
