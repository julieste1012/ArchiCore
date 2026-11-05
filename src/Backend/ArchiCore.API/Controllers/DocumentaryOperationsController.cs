using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using ArchiCore.API.Authorization;
using ArchiCore.Application.Abstractions.Persistence;
using ArchiCore.Application.Abstractions.Storage;
using ArchiCore.Application.Documentary;
using ArchiCore.Domain.Digitization;
using ArchiCore.Domain.Loans;
using ArchiCore.Domain.Warehouses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UglyToad.PdfPig;

namespace ArchiCore.API.Controllers;

public sealed record CreateBoxApiRequest(string BoxCode);
public sealed record BarcodeApiRequest(string Barcode);
public sealed record StoreBoxApiRequest(Guid WarehouseId, Guid LocationId);
public sealed record RelocateBoxApiRequest(Guid SourceWarehouseId, Guid DestinationWarehouseId, Guid DestinationLocationId);
public sealed record CreateLocationApiRequest(string Code, WarehouseLocationType Type, Guid? ParentLocationId, int? Capacity);
public sealed record AddCodeNameApiRequest(string Code, string Name);
public sealed record AddExpedientApiRequest(string Reference, string Title);
public sealed record CreateFolderApiRequest(string FolderCode, Guid ExpedientId);
public sealed record AddDocumentApiRequest(string Title, DateTime? DocumentDate);
public sealed record AddFolioApiRequest(int Number);
public sealed record AddAnnexApiRequest(string Title);
public sealed record RequestLoanApiRequest(DateTimeOffset DueAtUtc);

[Authorize]
[Route("")]
public sealed class DocumentaryOperationsController(
    DocumentaryManagementService management,
    DocumentaryWorkflowService workflows,
    ILoanRepository loans,
    IDigitizationRequestRepository digitizations,
    IObjectStorage objectStorage) : ApiControllerBase
{
    private const long MaxFileBytes = 40L * 1024 * 1024;
    private const long MaxBatchBytes = 80L * 1024 * 1024;
    private const int MaxFiles = 10;
    private const int MaxPdfPages = 10_000;

    [HttpGet("api/projects/{projectId:guid}/archive")]
    [Authorize(Policy = AppPolicies.ArchivesRead)]
    public async Task<IActionResult> GetArchive(Guid projectId, CancellationToken ct)
    {
        var archive = await management.GetArchiveByProjectAsync(projectId, ct);
        return archive is null ? NotFound() : Ok(archive);
    }

    [HttpPost("api/projects/{projectId:guid}/archive")]
    [Authorize(Policy = AppPolicies.ArchivesManage)]
    public async Task<IActionResult> CreateArchive(Guid projectId, CancellationToken ct)
    {
        var result = await workflows.CreateArchiveAsync(projectId, ct);
        return result.IsFailure ? ToProblem(result.Error) : StatusCode(201, new { id = result.Value, projectId });
    }

    [HttpPost("api/archives/{archiveId:guid}/sections")]
    [Authorize(Policy = AppPolicies.ArchivesManage)]
    public async Task<IActionResult> AddSection(Guid archiveId, AddCodeNameApiRequest request, CancellationToken ct)
        => ToResponse(await management.AddSectionAsync(archiveId, request.Code, request.Name, ct));

    [HttpPost("api/archives/{archiveId:guid}/sections/{sectionId:guid}/series")]
    [Authorize(Policy = AppPolicies.ArchivesManage)]
    public async Task<IActionResult> AddSeries(Guid archiveId, Guid sectionId, AddCodeNameApiRequest request, CancellationToken ct)
        => ToResponse(await management.AddSeriesAsync(archiveId, sectionId, request.Code, request.Name, ct));

    [HttpPost("api/archives/{archiveId:guid}/series/{seriesId:guid}/subseries")]
    [Authorize(Policy = AppPolicies.ArchivesManage)]
    public async Task<IActionResult> AddSubseries(Guid archiveId, Guid seriesId, AddCodeNameApiRequest request, CancellationToken ct)
        => ToResponse(await management.AddSubseriesAsync(archiveId, seriesId, request.Code, request.Name, ct));

    [HttpPost("api/archives/{archiveId:guid}/subseries/{subseriesId:guid}/expedients")]
    [Authorize(Policy = AppPolicies.ArchivesManage)]
    public async Task<IActionResult> AddExpedient(Guid archiveId, Guid subseriesId, AddExpedientApiRequest request, CancellationToken ct)
        => ToResponse(await management.AddExpedientAsync(archiveId, subseriesId, request.Reference, request.Title, ct));

    [HttpGet("api/warehouses/{warehouseId:guid}/locations")]
    [Authorize(Policy = AppPolicies.WarehousesRead)]
    public async Task<IActionResult> GetWarehouseLocations(Guid warehouseId, CancellationToken ct)
    {
        var locations = await management.GetWarehouseLocationsAsync(warehouseId, ct);
        return locations is null ? NotFound() : Ok(locations);
    }

    [HttpPost("api/warehouses/{warehouseId:guid}/locations")]
    [Authorize(Policy = AppPolicies.BoxesPhysical)]
    public async Task<IActionResult> AddWarehouseLocation(Guid warehouseId, CreateLocationApiRequest request, CancellationToken ct)
        => ToResponse(await management.AddWarehouseLocationAsync(
            warehouseId, request.Code, request.Type, request.ParentLocationId, request.Capacity, ct));

    [HttpGet("api/boxes")]
    [Authorize(Policy = AppPolicies.BoxesRead)]
    public async Task<IActionResult> ListBoxes(
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 20,
        CancellationToken ct = default)
        => Ok(await management.ListBoxesAsync(page, pageSize, ct));

    [HttpGet("api/boxes/{id:guid}")]
    [Authorize(Policy = AppPolicies.BoxesRead)]
    public async Task<IActionResult> GetBox(Guid id, CancellationToken ct)
    {
        var box = await management.GetBoxAsync(id, ct);
        return box is null ? NotFound() : Ok(box);
    }

    [HttpPost("api/boxes")]
    [Authorize(Policy = AppPolicies.BoxesCreate)]
    public async Task<IActionResult> CreateBox(CreateBoxApiRequest request, CancellationToken ct)
    {
        var result = await management.CreateBoxAsync(request.BoxCode, ct);
        return result.IsFailure ? ToProblem(result.Error) : StatusCode(201, new { id = result.Value });
    }

    [HttpPost("api/boxes/{id:guid}/barcode")]
    [Authorize(Policy = AppPolicies.BoxesCreate)]
    public async Task<IActionResult> AssignBarcode(Guid id, BarcodeApiRequest request, CancellationToken ct)
        => ToAction(await management.AssignBarcodeAsync(id, request.Barcode, ct));

    [HttpPost("api/boxes/{id:guid}/available")]
    [Authorize(Policy = AppPolicies.BoxesCreate)]
    public async Task<IActionResult> MakeBoxAvailable(Guid id, CancellationToken ct)
        => ToAction(await management.MakeBoxAvailableAsync(id, ct));

    [HttpPost("api/boxes/{id:guid}/store")]
    [Authorize(Policy = AppPolicies.BoxesPhysical)]
    public async Task<IActionResult> StoreBox(Guid id, StoreBoxApiRequest request, CancellationToken ct)
        => ToAction(await management.StoreBoxAsync(id, request.WarehouseId, request.LocationId, ct));

    [HttpPost("api/boxes/{id:guid}/relocate")]
    [Authorize(Policy = AppPolicies.BoxesPhysical)]
    public async Task<IActionResult> RelocateBox(Guid id, RelocateBoxApiRequest request, CancellationToken ct)
        => ToAction(await management.MoveBoxAsync(
            id, request.SourceWarehouseId, request.DestinationWarehouseId, request.DestinationLocationId, ct));

    [HttpPost("api/boxes/{id:guid}/move-out")]
    [Authorize(Policy = AppPolicies.BoxesPhysical)]
    public async Task<IActionResult> MoveBoxOut(Guid id, [FromQuery] Guid warehouseId, CancellationToken ct)
        => ToAction(await management.MoveBoxOutOfStorageAsync(id, warehouseId, ct));

    [HttpGet("api/archives/{archiveId:guid}/folders")]
    [Authorize(Policy = AppPolicies.FoldersRead)]
    public async Task<IActionResult> ListFolders(
        Guid archiveId,
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 20,
        CancellationToken ct = default)
        => Ok(await management.ListFoldersAsync(archiveId, page, pageSize, ct));

    [HttpPost("api/archives/{archiveId:guid}/folders")]
    [Authorize(Policy = AppPolicies.FoldersManage)]
    public async Task<IActionResult> CreateFolder(Guid archiveId, CreateFolderApiRequest request, CancellationToken ct)
    {
        var result = await workflows.CreateFolderAsync(request.FolderCode, archiveId, request.ExpedientId, ct);
        return result.IsFailure ? ToProblem(result.Error) : StatusCode(201, new { id = result.Value, archiveId });
    }

    [HttpGet("api/folders/{id:guid}")]
    [Authorize(Policy = AppPolicies.FoldersRead)]
    public async Task<IActionResult> GetFolder(Guid id, CancellationToken ct)
    {
        var folder = await management.GetFolderAsync(id, ct);
        return folder is null ? NotFound() : Ok(folder);
    }

    [HttpPost("api/folders/{id:guid}/receive")]
    [Authorize(Policy = AppPolicies.FoldersManage)]
    public async Task<IActionResult> ReceiveFolder(Guid id, CancellationToken ct)
        => ToAction(await management.ReceiveFolderAsync(id, ct));

    [HttpPost("api/folders/{id:guid}/organize")]
    [Authorize(Policy = AppPolicies.FoldersManage)]
    public async Task<IActionResult> OrganizeFolder(Guid id, CancellationToken ct)
        => ToAction(await management.OrganizeFolderAsync(id, ct));

    [HttpPost("api/folders/{id:guid}/documents")]
    [Authorize(Policy = AppPolicies.FoldersManage)]
    public async Task<IActionResult> AddDocument(Guid id, AddDocumentApiRequest request, CancellationToken ct)
        => ToResponse(await management.AddFolderDocumentAsync(id, request.Title, request.DocumentDate, ct));

    [HttpPost("api/folders/{folderId:guid}/documents/{documentId:guid}/folios")]
    [Authorize(Policy = AppPolicies.FoldersManage)]
    public async Task<IActionResult> AddFolio(Guid folderId, Guid documentId, AddFolioApiRequest request, CancellationToken ct)
        => ToResponse(await management.AddFolioAsync(folderId, documentId, request.Number, ct));

    [HttpPost("api/folders/{folderId:guid}/documents/{documentId:guid}/annexes")]
    [Authorize(Policy = AppPolicies.FoldersManage)]
    public async Task<IActionResult> AddAnnex(Guid folderId, Guid documentId, AddAnnexApiRequest request, CancellationToken ct)
        => ToResponse(await management.AddAnnexAsync(folderId, documentId, request.Title, ct));

    [HttpPost("api/folders/{folderId:guid}/box/{boxId:guid}")]
    [Authorize(Policy = AppPolicies.FoldersPhysical)]
    public async Task<IActionResult> AssignFolderToBox(Guid folderId, Guid boxId, CancellationToken ct)
        => ToAction(await workflows.AssignFolderToBoxAsync(folderId, boxId, ct));

    [HttpPost("api/folders/{folderId:guid}/move-to-box/{boxId:guid}")]
    [Authorize(Policy = AppPolicies.FoldersPhysical)]
    public async Task<IActionResult> MoveFolderToBox(Guid folderId, Guid boxId, CancellationToken ct)
        => ToAction(await workflows.MoveFolderToBoxAsync(folderId, boxId, ct));

    [HttpPost("api/projects/{projectId:guid}/folders/{folderId:guid}/loans")]
    [Authorize(Policy = AppPolicies.LoansRequest)]
    public async Task<IActionResult> RequestLoan(Guid projectId, Guid folderId, RequestLoanApiRequest request, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();
        var result = await workflows.RequestLoanAsync(projectId, folderId, userId, request.DueAtUtc, ct);
        return result.IsFailure ? ToProblem(result.Error) : StatusCode(201, new { id = result.Value, folderId });
    }

    [HttpGet("api/folders/{folderId:guid}/loans")]
    [Authorize(Policy = AppPolicies.LoansRead)]
    public async Task<IActionResult> ListLoans(Guid folderId, CancellationToken ct)
    {
        var items = await loans.ListByFolderAsync(folderId, 0, 100, ct);
        return Ok(items.Select(item => new
        {
            item.Id, item.ProjectId, item.FolderId, item.RequestedByUserId,
            item.RequestedAtUtc, item.DueAtUtc, item.Status, item.AuthorizedByUserId,
            item.AuthorizedAtUtc, item.DeliveredByUserId, item.DeliveredAtUtc,
            item.ReturnedByUserId, item.ReturnedAtUtc, item.OverdueDetectedAtUtc
        }));
    }

    [HttpPost("api/loans/{id:guid}/authorize")]
    [Authorize(Policy = AppPolicies.LoansAuthorize)]
    public async Task<IActionResult> AuthorizeLoan(Guid id, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();
        return ToAction(await workflows.AuthorizeLoanAsync(id, userId, ct));
    }

    [HttpPost("api/loans/{id:guid}/deliver")]
    [Authorize(Policy = AppPolicies.LoansDeliver)]
    public async Task<IActionResult> DeliverLoan(Guid id, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();
        return ToAction(await workflows.DeliverLoanAsync(id, userId, ct));
    }

    [HttpPost("api/loans/{id:guid}/return")]
    [Authorize(Policy = AppPolicies.LoansDeliver)]
    public async Task<IActionResult> ReturnLoan(Guid id, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();
        return ToAction(await workflows.ReturnLoanAsync(id, userId, DateTimeOffset.UtcNow, ct));
    }

    [HttpPost("api/folders/{folderId:guid}/digitizations")]
    [Authorize(Policy = AppPolicies.DigitizationRequest)]
    public async Task<IActionResult> RequestDigitization(Guid folderId, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();
        var folder = await management.GetFolderAsync(folderId, ct);
        if (folder is null) return NotFound();
        var projectId = await management.GetProjectIdForArchiveAsync(folder.ArchiveId, ct);
        if (projectId is null) return NotFound();
        var result = await workflows.RequestDigitizationAsync(projectId.Value, folderId, userId, ct);
        return result.IsFailure ? ToProblem(result.Error) : StatusCode(201, new { id = result.Value, folderId });
    }

    [HttpGet("api/folders/{folderId:guid}/digitizations")]
    [Authorize(Policy = AppPolicies.DigitizationRead)]
    public async Task<IActionResult> ListDigitizations(Guid folderId, CancellationToken ct)
    {
        var items = await digitizations.ListByFolderAsync(folderId, 0, 100, ct);
        return Ok(items.Select(item => new
        {
            item.Id, item.ProjectId, item.FolderId, item.RequestedByUserId, item.RequestedAtUtc,
            item.Status, item.StartedByUserId, item.StartedAtUtc, item.CompletedByUserId,
            item.CompletedAtUtc,
            Files = item.Files.Select(file => new
            {
                file.Id, file.OriginalFileName, file.ContentType, file.SizeBytes, file.Sha256, file.PageCount
            })
        }));
    }

    [HttpPost("api/digitizations/{id:guid}/start")]
    [Authorize(Policy = AppPolicies.DigitizationOperate)]
    public async Task<IActionResult> StartDigitization(Guid id, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();
        return ToAction(await workflows.StartDigitizationAsync(id, userId, ct));
    }

    [HttpPost("api/digitizations/{id:guid}/files")]
    [Authorize(Policy = AppPolicies.DigitizationOperate)]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxBatchBytes + 1024 * 1024)]
    public async Task<IActionResult> UploadDigitizedFiles(
        Guid id,
        [FromForm(Name = "files")] List<IFormFile> files,
        CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();

        var request = await digitizations.GetByIdAsync(id, ct);
        if (request is null) return NotFound();
        if (request.Status != DigitizationRequestStatus.Processing)
            return UnprocessableEntity(new ProblemDetails
            {
                Title = "Digitization.InvalidCompletionState",
                Detail = "La solicitud debe estar en Processing antes de cargar los archivos.",
                Status = StatusCodes.Status422UnprocessableEntity
            });

        if (files is null || files.Count is < 1 or > MaxFiles)
            return UnprocessableEntity(new ProblemDetails
            {
                Title = "Digitization.InvalidFileCount",
                Detail = $"Debes cargar entre 1 y {MaxFiles} archivos PDF.",
                Status = StatusCodes.Status422UnprocessableEntity
            });

        var totalBytes = files.Sum(file => file.Length);
        if (totalBytes > MaxBatchBytes)
            return StatusCode(StatusCodes.Status413PayloadTooLarge);

        var storedKeys = new List<string>();
        var inputs = new List<DigitizedFileInput>();

        try
        {
            foreach (var file in files)
            {
                if (file.Length is <= 0 or > MaxFileBytes)
                    return await RejectAndCleanupAsync(storedKeys, "Digitization.InvalidFileSize",
                        $"Cada archivo debe pesar entre 1 byte y {MaxFileBytes / 1024 / 1024} MiB. Archivo: {file.FileName}", ct);

                var originalName = Path.GetFileName(file.FileName.Replace('\\', '/')).Trim();
                if (string.IsNullOrWhiteSpace(originalName) || originalName.Length > 255
                    || originalName.Any(char.IsControl)
                    || !string.Equals(Path.GetExtension(originalName), ".pdf", StringComparison.OrdinalIgnoreCase))
                    return await RejectAndCleanupAsync(storedKeys, "Digitization.InvalidFileName",
                        "Solo se admiten nombres de archivo PDF válidos de hasta 255 caracteres.", ct);

                await using var stream = file.OpenReadStream();
                var signature = new byte[5];
                var read = await stream.ReadAsync(signature, ct);
                if (read != signature.Length || !signature.AsSpan().SequenceEqual("%PDF-"u8))
                    return await RejectAndCleanupAsync(storedKeys, "Digitization.InvalidPdf",
                        $"El contenido de {originalName} no corresponde a un PDF.", ct);

                stream.Position = 0;
                int pageCount;
                try
                {
                    using var pdf = PdfDocument.Open(stream);
                    pageCount = pdf.NumberOfPages;
                }
                catch (Exception)
                {
                    return await RejectAndCleanupAsync(storedKeys, "Digitization.InvalidPdf",
                        $"No fue posible interpretar el PDF {originalName}. Puede estar dañado o protegido.", ct);
                }

                if (pageCount is <= 0 or > MaxPdfPages)
                    return await RejectAndCleanupAsync(storedKeys, "Digitization.InvalidPageCount",
                        $"El PDF debe contener entre 1 y {MaxPdfPages} páginas.", ct);

                stream.Position = 0;
                var checksum = Convert.ToHexString(await SHA256.HashDataAsync(stream, ct)).ToLowerInvariant();
                stream.Position = 0;

                var objectKey = $"digitization/{id:N}/{Guid.NewGuid():N}.pdf";
                await objectStorage.StoreAsync(objectKey, stream, file.Length, "application/pdf", ct);
                storedKeys.Add(objectKey);
                inputs.Add(new DigitizedFileInput(
                    objectKey, originalName, "application/pdf", file.Length, checksum, pageCount));
            }

            var result = await workflows.CompleteDigitizationAsync(id, userId, inputs, ct);
            if (result.IsFailure)
                return await RejectAndCleanupAsync(storedKeys, result.Error.Code, result.Error.Message, ct);

            return Ok(new { id, status = "Completed", fileCount = inputs.Count });
        }
        catch (OperationCanceledException)
        {
            await CleanupAsync(storedKeys);
            throw;
        }
        catch (Exception)
        {
            await CleanupAsync(storedKeys);
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new ProblemDetails
            {
                Title = "Digitization.StorageOrProcessingFailure",
                Detail = "No fue posible completar la carga. No se registró la digitalización.",
                Status = StatusCodes.Status503ServiceUnavailable
            });
        }
    }

    private bool TryGetCurrentUserId(out Guid userId)
        => Guid.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out userId);

    private IActionResult ToAction(ArchiCore.Domain.Common.Result result)
        => result.IsFailure ? ToProblem(result.Error) : NoContent();

    private async Task<IActionResult> RejectAndCleanupAsync(
        IEnumerable<string> keys, string code, string detail, CancellationToken ct)
    {
        await CleanupAsync(keys);
        return UnprocessableEntity(new ProblemDetails
        {
            Title = code,
            Detail = detail,
            Status = StatusCodes.Status422UnprocessableEntity
        });
    }

    private async Task CleanupAsync(IEnumerable<string> keys)
    {
        foreach (var key in keys)
        {
            try { await objectStorage.DeleteAsync(key, CancellationToken.None); }
            catch { /* Best-effort cleanup; no key is exposed to the client. */ }
        }
    }
}
