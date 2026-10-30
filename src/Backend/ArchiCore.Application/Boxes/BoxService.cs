using ArchiCore.Application.Abstractions.Persistence;
using ArchiCore.Application.Common;
using ArchiCore.Domain.Boxes;
using ArchiCore.Domain.Common;
using ArchiCore.Domain.ValueObjects;

namespace ArchiCore.Application.Boxes;

public sealed class BoxService(
    IBoxRepository boxes,
    IUnitOfWork unitOfWork)
{
    public async Task<Result<PagedResult<BoxDto>>> ListAsync(
        BoxStatus? status,
        string? barcode,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        (page, pageSize) = NormalizePage(page, pageSize);
        Barcode? barcodeValue = null;
        if (!string.IsNullOrWhiteSpace(barcode))
        {
            var barcodeResult = Barcode.Create(barcode);
            if (barcodeResult.IsFailure)
            {
                return Result.Failure<PagedResult<BoxDto>>(barcodeResult.Error);
            }

            barcodeValue = barcodeResult.Value;
        }

        var skipLong = ((long)page - 1L) * pageSize;
        var skip = (int)Math.Min(skipLong, int.MaxValue);
        var items = await boxes.ListAsync(status, barcodeValue, skip, pageSize, cancellationToken);
        var totalCount = await boxes.CountAsync(status, barcodeValue, cancellationToken);

        return Result.Success(new PagedResult<BoxDto>(
            page, pageSize, totalCount, items.Select(ToDto).ToArray()));
    }

    public async Task<Result<BoxDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var box = await boxes.GetByIdAsync(id, cancellationToken);
        return box is null
            ? Result.Failure<BoxDto>(new Error("Box.NotFound", "No se encontró la caja solicitada."))
            : Result.Success(ToDto(box));
    }

    public async Task<Result<BoxDto>> CreateAsync(
        CreateBoxRequest request,
        CancellationToken cancellationToken = default)
    {
        var codeResult = BoxCode.Create(request.BoxCode);
        if (codeResult.IsFailure)
        {
            return Result.Failure<BoxDto>(codeResult.Error);
        }

        if (await boxes.GetByCodeAsync(codeResult.Value, cancellationToken) is not null)
        {
            return Result.Failure<BoxDto>(
                new Error("Box.DuplicateCode", "Ya existe una caja con ese código."));
        }

        var createResult = Box.Create(codeResult.Value);
        if (createResult.IsFailure)
        {
            return Result.Failure<BoxDto>(createResult.Error);
        }

        await boxes.AddAsync(createResult.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(ToDto(createResult.Value));
    }

    public async Task<Result<BoxDto>> AssignBarcodeAsync(
        Guid id,
        AssignBoxBarcodeRequest request,
        CancellationToken cancellationToken = default)
    {
        var box = await boxes.GetByIdAsync(id, cancellationToken);
        if (box is null)
        {
            return Result.Failure<BoxDto>(
                new Error("Box.NotFound", "No se encontró la caja solicitada."));
        }

        var barcodeResult = Barcode.Create(request.Barcode);
        if (barcodeResult.IsFailure)
        {
            return Result.Failure<BoxDto>(barcodeResult.Error);
        }

        var existing = await boxes.GetByBarcodeAsync(barcodeResult.Value, cancellationToken);
        if (existing is not null && existing.Id != box.Id)
        {
            return Result.Failure<BoxDto>(
                new Error("Box.DuplicateBarcode", "El código de barras ya está asignado a otra caja."));
        }

        var assignResult = box.AssignBarcode(barcodeResult.Value);
        if (assignResult.IsFailure)
        {
            return Result.Failure<BoxDto>(assignResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(ToDto(box));
    }

    private static BoxDto ToDto(Box box)
        => new(
            box.Id,
            box.BoxCode.Value,
            box.Barcode?.Value,
            box.Status,
            box.CreatedAt,
            box.UpdatedAt);

    private static (int Page, int PageSize) NormalizePage(int page, int pageSize)
        => (Math.Max(1, page), Math.Clamp(pageSize, 1, 100));
}
