using ArchiCore.Application.Abstractions.Persistence;
using ArchiCore.Application.Common;
using ArchiCore.Domain.Archives;
using ArchiCore.Domain.Boxes;
using ArchiCore.Domain.Common;
using ArchiCore.Domain.Folders;
using ArchiCore.Domain.ValueObjects;
using ArchiCore.Domain.Warehouses;

namespace ArchiCore.Application.Documentary;

public sealed record ArchiveDto(
    Guid Id,
    Guid ProjectId,
    IReadOnlyList<ArchiveSectionDto> Sections);

public sealed record ArchiveSectionDto(
    Guid Id,
    string Code,
    string Name,
    IReadOnlyList<ArchiveSeriesDto> Series);

public sealed record ArchiveSeriesDto(
    Guid Id,
    string Code,
    string Name,
    IReadOnlyList<ArchiveSubseriesDto> Subseries);

public sealed record ArchiveSubseriesDto(
    Guid Id,
    string Code,
    string Name,
    IReadOnlyList<ExpedientDto> Expedients);

public sealed record ExpedientDto(Guid Id, string Reference, string Title);

public sealed record BoxDto(
    Guid Id,
    string BoxCode,
    string? Barcode,
    BoxStatus Status,
    Guid? WarehouseLocationId,
    IReadOnlyList<Guid> FolderIds,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record WarehouseLocationDto(
    Guid Id,
    string Code,
    WarehouseLocationType Type,
    Guid? ParentLocationId,
    int? Capacity,
    int OccupiedCapacity,
    IReadOnlyList<Guid> BoxIds);

public sealed record FolderDto(
    Guid Id,
    string FolderCode,
    Guid ArchiveId,
    Guid ExpedientId,
    Guid? BoxId,
    Guid? LoanId,
    FolderStatus Status,
    IReadOnlyList<FolderDocumentDto> Documents,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record FolderDocumentDto(
    Guid Id,
    string Title,
    DateTime? DocumentDate,
    IReadOnlyList<int> Folios,
    IReadOnlyList<FolderAnnexDto> Annexes);

public sealed record FolderAnnexDto(string Title, int Sequence);

/// <summary>
/// Read models and single-aggregate commands for the documentary catalog and physical storage.
/// Cross-aggregate transactions are kept in DocumentaryWorkflowService.
/// </summary>
public sealed class DocumentaryManagementService(
    IArchiveRepository archives,
    IBoxRepository boxes,
    IFolderRepository folders,
    IWarehouseRepository warehouses,
    IUnitOfWork unitOfWork)
{
    public async Task<ArchiveDto?> GetArchiveByProjectAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        var archive = await archives.GetByProjectIdAsync(projectId, cancellationToken);
        return archive is null ? null : Map(archive);
    }

    public async Task<Guid?> GetProjectIdForArchiveAsync(Guid archiveId, CancellationToken cancellationToken = default)
        => (await archives.GetByIdAsync(archiveId, cancellationToken))?.ProjectId;

    public async Task<Result<Guid>> AddSectionAsync(Guid archiveId, string code, string name, CancellationToken cancellationToken = default)
    {
        var archive = await archives.GetByIdAsync(archiveId, cancellationToken);
        if (archive is null) return Result.Failure<Guid>(new Error("Archive.NotFound", "No se encontró el Fondo documental."));
        var result = archive.AddSection(code, name);
        if (result.IsFailure) return result;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return result;
    }

    public async Task<Result<Guid>> AddSeriesAsync(Guid archiveId, Guid sectionId, string code, string name, CancellationToken cancellationToken = default)
    {
        var archive = await archives.GetByIdAsync(archiveId, cancellationToken);
        if (archive is null) return Result.Failure<Guid>(new Error("Archive.NotFound", "No se encontró el Fondo documental."));
        var result = archive.AddSeries(sectionId, code, name);
        if (result.IsFailure) return result;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return result;
    }

    public async Task<Result<Guid>> AddSubseriesAsync(Guid archiveId, Guid seriesId, string code, string name, CancellationToken cancellationToken = default)
    {
        var archive = await archives.GetByIdAsync(archiveId, cancellationToken);
        if (archive is null) return Result.Failure<Guid>(new Error("Archive.NotFound", "No se encontró el Fondo documental."));
        var result = archive.AddSubseries(seriesId, code, name);
        if (result.IsFailure) return result;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return result;
    }

    public async Task<Result<Guid>> AddExpedientAsync(Guid archiveId, Guid subseriesId, string reference, string title, CancellationToken cancellationToken = default)
    {
        var archive = await archives.GetByIdAsync(archiveId, cancellationToken);
        if (archive is null) return Result.Failure<Guid>(new Error("Archive.NotFound", "No se encontró el Fondo documental."));
        var result = archive.AddExpedient(subseriesId, reference, title);
        if (result.IsFailure) return result;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return result;
    }

    public async Task<PagedResult<BoxDto>> ListBoxesAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var total = await boxes.CountAsync(cancellationToken);
        var items = await boxes.ListAsync((page - 1) * pageSize, pageSize, cancellationToken);
        return new PagedResult<BoxDto>(page, pageSize, total, items.Select(Map).ToArray());
    }

    public async Task<BoxDto?> GetBoxAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var box = await boxes.GetByIdAsync(id, cancellationToken);
        return box is null ? null : Map(box);
    }

    public async Task<PagedResult<FolderDto>> ListFoldersAsync(Guid archiveId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var total = await folders.CountByArchiveAsync(archiveId, cancellationToken);
        var items = await folders.ListByArchiveAsync(archiveId, (page - 1) * pageSize, pageSize, cancellationToken);
        return new PagedResult<FolderDto>(page, pageSize, total, items.Select(Map).ToArray());
    }

    public async Task<FolderDto?> GetFolderAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var folder = await folders.GetByIdAsync(id, cancellationToken);
        return folder is null ? null : Map(folder);
    }

    public async Task<IReadOnlyList<WarehouseLocationDto>?> GetWarehouseLocationsAsync(Guid warehouseId, CancellationToken cancellationToken = default)
    {
        var warehouse = await warehouses.GetByIdAsync(warehouseId, cancellationToken);
        return warehouse?.Locations
            .OrderBy(location => location.Type)
            .ThenBy(location => location.Code, StringComparer.Ordinal)
            .Select(location => new WarehouseLocationDto(
                location.Id,
                location.Code,
                location.Type,
                location.ParentLocationId,
                location.Capacity,
                location.OccupiedCapacity,
                location.BoxIds.ToArray()))
            .ToArray();
    }

    public async Task<Result<Guid>> AddWarehouseLocationAsync(
        Guid warehouseId,
        string code,
        WarehouseLocationType type,
        Guid? parentLocationId,
        int? capacity,
        CancellationToken cancellationToken = default)
    {
        var warehouse = await warehouses.GetByIdAsync(warehouseId, cancellationToken);
        if (warehouse is null) return Result.Failure<Guid>(new Error("Warehouse.NotFound", "No se encontró la bodega."));
        var result = warehouse.AddLocation(code, type, parentLocationId, capacity);
        if (result.IsFailure) return result;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return result;
    }

    public async Task<Result<Guid>> CreateBoxAsync(string boxCode, CancellationToken cancellationToken = default)
    {
        var code = BoxCode.Create(boxCode);
        if (code.IsFailure) return Result.Failure<Guid>(code.Error);
        if (await boxes.GetByCodeAsync(code.Value, cancellationToken) is not null)
            return Result.Failure<Guid>(new Error("Box.DuplicateCode", "Ya existe una caja con ese código."));
        var result = Box.Create(code.Value);
        if (result.IsFailure) return Result.Failure<Guid>(result.Error);
        await boxes.AddAsync(result.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(result.Value.Id);
    }

    public async Task<Result> AssignBarcodeAsync(Guid boxId, string barcode, CancellationToken cancellationToken = default)
    {
        var box = await boxes.GetByIdAsync(boxId, cancellationToken);
        if (box is null) return Result.Failure(new Error("Box.NotFound", "No se encontró la caja."));
        var parsed = Barcode.Create(barcode);
        if (parsed.IsFailure) return Result.Failure(parsed.Error);
        var existing = await boxes.GetByBarcodeAsync(parsed.Value, cancellationToken);
        if (existing is not null && existing.Id != boxId)
            return Result.Failure(new Error("Box.DuplicateBarcode", "El código de barras ya está asignado a otra caja."));
        var result = box.AssignBarcode(parsed.Value);
        if (result.IsFailure) return result;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> MakeBoxAvailableAsync(Guid boxId, CancellationToken cancellationToken = default)
    {
        var box = await boxes.GetByIdAsync(boxId, cancellationToken);
        if (box is null) return Result.Failure(new Error("Box.NotFound", "No se encontró la caja."));
        var result = box.MakeAvailable();
        if (result.IsFailure) return result;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> StoreBoxAsync(Guid boxId, Guid warehouseId, Guid locationId, CancellationToken cancellationToken = default)
    {
        var box = await boxes.GetByIdAsync(boxId, cancellationToken);
        if (box is null) return Result.Failure(new Error("Box.NotFound", "No se encontró la caja."));
        var warehouse = await warehouses.GetByIdAsync(warehouseId, cancellationToken);
        if (warehouse is null) return Result.Failure(new Error("Warehouse.NotFound", "No se encontró la bodega."));
        var occupancy = warehouse.OccupyLocation(locationId, boxId);
        if (occupancy.IsFailure) return occupancy;
        var stored = box.Store(locationId);
        if (stored.IsFailure) return stored;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> MoveBoxAsync(
        Guid boxId,
        Guid sourceWarehouseId,
        Guid destinationWarehouseId,
        Guid destinationLocationId,
        CancellationToken cancellationToken = default)
    {
        var box = await boxes.GetByIdAsync(boxId, cancellationToken);
        if (box is null) return Result.Failure(new Error("Box.NotFound", "No se encontró la caja."));
        if (box.Status != BoxStatus.InStorage || box.WarehouseLocationId is null)
            return Result.Failure(new Error("Box.NotInStorage", "La caja no está almacenada en una ubicación física."));

        var source = await warehouses.GetByIdAsync(sourceWarehouseId, cancellationToken);
        if (source is null) return Result.Failure(new Error("Warehouse.SourceNotFound", "No se encontró la bodega de origen."));
        var destination = sourceWarehouseId == destinationWarehouseId
            ? source
            : await warehouses.GetByIdAsync(destinationWarehouseId, cancellationToken);
        if (destination is null) return Result.Failure(new Error("Warehouse.DestinationNotFound", "No se encontró la bodega de destino."));

        var sourceLocationId = box.WarehouseLocationId.Value;
        var sourceContainsBox = source.Locations.Any(location =>
            location.Id == sourceLocationId && location.BoxIds.Contains(boxId));
        if (!sourceContainsBox)
            return Result.Failure(new Error("Box.SourceLocationMismatch", "La ubicación registrada en la caja no coincide con la ocupación de la bodega de origen."));

        Result movement;
        if (sourceWarehouseId == destinationWarehouseId)
        {
            movement = source.MoveBox(boxId, destinationLocationId);
        }
        else
        {
            movement = source.ReleaseLocation(sourceLocationId, boxId);
            if (movement.IsSuccess)
                movement = destination.OccupyLocation(destinationLocationId, boxId);
        }

        if (movement.IsFailure) return movement;
        var relocated = box.Relocate(destinationLocationId);
        if (relocated.IsFailure) return relocated;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> MoveBoxOutOfStorageAsync(Guid boxId, Guid warehouseId, CancellationToken cancellationToken = default)
    {
        var box = await boxes.GetByIdAsync(boxId, cancellationToken);
        if (box is null) return Result.Failure(new Error("Box.NotFound", "No se encontró la caja."));
        if (box.WarehouseLocationId is null)
            return Result.Failure(new Error("Box.NotInStorage", "La caja no tiene una ubicación física registrada."));
        if (box.Status != BoxStatus.InStorage)
            return Result.Failure(new Error("Box.NotInStorage", "La caja debe estar almacenada antes de salir de su ubicación."));
        var warehouse = await warehouses.GetByIdAsync(warehouseId, cancellationToken);
        if (warehouse is null) return Result.Failure(new Error("Warehouse.NotFound", "No se encontró la bodega."));
        var release = warehouse.ReleaseLocation(box.WarehouseLocationId.Value, boxId);
        if (release.IsFailure) return release;
        var moved = box.Move();
        if (moved.IsFailure) return moved;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> ReceiveFolderAsync(Guid folderId, CancellationToken cancellationToken = default)
        => await ChangeFolderAsync(folderId, folder => folder.Receive(), cancellationToken);

    public async Task<Result> OrganizeFolderAsync(Guid folderId, CancellationToken cancellationToken = default)
        => await ChangeFolderAsync(folderId, folder => folder.Organize(), cancellationToken);

    public async Task<Result<Guid>> AddFolderDocumentAsync(Guid folderId, string title, DateTime? documentDate, CancellationToken cancellationToken = default)
    {
        var folder = await folders.GetByIdAsync(folderId, cancellationToken);
        if (folder is null) return Result.Failure<Guid>(new Error("Folder.NotFound", "No se encontró la carpeta."));
        var result = folder.AddDocument(title, documentDate);
        if (result.IsFailure) return result;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return result;
    }

    public async Task<Result<int>> AddFolioAsync(Guid folderId, Guid documentId, int number, CancellationToken cancellationToken = default)
    {
        var folder = await folders.GetByIdAsync(folderId, cancellationToken);
        if (folder is null) return Result.Failure<int>(new Error("Folder.NotFound", "No se encontró la carpeta."));
        var result = folder.AddFolio(documentId, number);
        if (result.IsFailure) return result;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return result;
    }

    public async Task<Result<int>> AddAnnexAsync(Guid folderId, Guid documentId, string title, CancellationToken cancellationToken = default)
    {
        var folder = await folders.GetByIdAsync(folderId, cancellationToken);
        if (folder is null) return Result.Failure<int>(new Error("Folder.NotFound", "No se encontró la carpeta."));
        var result = folder.AddAnnex(documentId, title);
        if (result.IsFailure) return result;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return result;
    }

    private async Task<Result> ChangeFolderAsync(Guid folderId, Func<Folder, Result> operation, CancellationToken cancellationToken)
    {
        var folder = await folders.GetByIdAsync(folderId, cancellationToken);
        if (folder is null) return Result.Failure(new Error("Folder.NotFound", "No se encontró la carpeta."));
        var result = operation(folder);
        if (result.IsFailure) return result;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private static ArchiveDto Map(Archive archive)
        => new(
            archive.Id,
            archive.ProjectId,
            archive.Sections.Select(section => new ArchiveSectionDto(
                section.Id,
                section.Code,
                section.Name,
                section.Series.Select(series => new ArchiveSeriesDto(
                    series.Id,
                    series.Code,
                    series.Name,
                    series.Subseries.Select(subseries => new ArchiveSubseriesDto(
                        subseries.Id,
                        subseries.Code,
                        subseries.Name,
                        subseries.Expedients.Select(expedient => new ExpedientDto(
                            expedient.Id,
                            expedient.Reference,
                            expedient.Title)).ToArray())).ToArray())).ToArray())).ToArray());

    private static BoxDto Map(Box box)
        => new(box.Id, box.BoxCode.Value, box.Barcode?.Value, box.Status, box.WarehouseLocationId,
            box.FolderIds.ToArray(), box.CreatedAt, box.UpdatedAt);

    private static FolderDto Map(Folder folder)
        => new(folder.Id, folder.FolderCode.Value, folder.ArchiveId, folder.ExpedientId,
            folder.BoxId, folder.LoanId, folder.Status,
            folder.Documents.Select(document => new FolderDocumentDto(
                document.Id,
                document.Title,
                document.DocumentDate,
                document.Folios.Select(folio => folio.Number).ToArray(),
                document.Annexes.Select(annex => new FolderAnnexDto(annex.Title, annex.Sequence)).ToArray())).ToArray(),
            folder.CreatedAt, folder.UpdatedAt);
}
