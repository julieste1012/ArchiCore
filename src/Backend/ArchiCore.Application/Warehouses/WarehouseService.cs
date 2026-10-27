using ArchiCore.Application.Abstractions.Persistence;
using ArchiCore.Application.Common;
using ArchiCore.Domain.Common;
using ArchiCore.Domain.ValueObjects;
using ArchiCore.Domain.Warehouses;

namespace ArchiCore.Application.Warehouses;

public sealed class WarehouseService(
    IWarehouseRepository warehouses,
    IUnitOfWork unitOfWork)
{
    public async Task<PagedResult<WarehouseDto>> ListAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        (page, pageSize) = NormalizePage(page, pageSize);
        var skip = (page - 1) * pageSize;
        var items = await warehouses.ListAsync(skip, pageSize, cancellationToken);
        var count = await warehouses.CountAsync(cancellationToken);
        return new PagedResult<WarehouseDto>(page, pageSize, count, items.Select(ToDto).ToArray());
    }

    public async Task<Result<WarehouseDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var warehouse = await warehouses.GetByIdAsync(id, cancellationToken);
        return warehouse is null
            ? Result.Failure<WarehouseDto>(new Error("Warehouse.NotFound", "No se encontró la bodega solicitada."))
            : Result.Success(ToDto(warehouse));
    }

    public async Task<Result<WarehouseDto>> CreateAsync(CreateWarehouseRequest request, CancellationToken cancellationToken = default)
    {
        var codeResult = WarehouseCode.Create(request.WarehouseCode);
        if (codeResult.IsFailure) return Result.Failure<WarehouseDto>(codeResult.Error);

        if (await warehouses.GetByCodeAsync(codeResult.Value, cancellationToken) is not null)
            return Result.Failure<WarehouseDto>(
                new Error("Warehouse.DuplicateCode", "Ya existe una bodega con ese código."));

        var createResult = Warehouse.Create(codeResult.Value);
        if (createResult.IsFailure) return Result.Failure<WarehouseDto>(createResult.Error);

        await warehouses.AddAsync(createResult.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(ToDto(createResult.Value));
    }

    public Task<Result<WarehouseDto>> ActivateAsync(Guid id, CancellationToken cancellationToken = default)
        => ExecuteTransitionAsync(id, warehouse => warehouse.Activate(), cancellationToken);
    public Task<Result<WarehouseDto>> StartMaintenanceAsync(Guid id, CancellationToken cancellationToken = default)
        => ExecuteTransitionAsync(id, warehouse => warehouse.StartMaintenance(), cancellationToken);
    public Task<Result<WarehouseDto>> FinishMaintenanceAsync(Guid id, CancellationToken cancellationToken = default)
        => ExecuteTransitionAsync(id, warehouse => warehouse.FinishMaintenance(), cancellationToken);
    public Task<Result<WarehouseDto>> CloseAsync(Guid id, CancellationToken cancellationToken = default)
        => ExecuteTransitionAsync(id, warehouse => warehouse.Close(), cancellationToken);
    public Task<Result<WarehouseDto>> ArchiveAsync(Guid id, CancellationToken cancellationToken = default)
        => ExecuteTransitionAsync(id, warehouse => warehouse.Archive(), cancellationToken);

    private async Task<Result<WarehouseDto>> ExecuteTransitionAsync(
        Guid id, Func<Warehouse, Result> transition, CancellationToken cancellationToken)
    {
        var warehouse = await warehouses.GetByIdAsync(id, cancellationToken);
        if (warehouse is null)
            return Result.Failure<WarehouseDto>(new Error("Warehouse.NotFound", "No se encontró la bodega solicitada."));

        var result = transition(warehouse);
        if (result.IsFailure) return Result.Failure<WarehouseDto>(result.Error);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(ToDto(warehouse));
    }

    private static WarehouseDto ToDto(Warehouse warehouse)
        => new(warehouse.Id, warehouse.WarehouseCode.Value, warehouse.Status, warehouse.CreatedAt, warehouse.UpdatedAt);

    private static (int Page, int PageSize) NormalizePage(int page, int pageSize)
        => (Math.Max(1, page), Math.Clamp(pageSize, 1, 100));
}
