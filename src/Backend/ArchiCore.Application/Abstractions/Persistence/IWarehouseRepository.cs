using ArchiCore.Domain.ValueObjects;
using ArchiCore.Domain.Warehouses;

namespace ArchiCore.Application.Abstractions.Persistence;

public interface IWarehouseRepository
{
    Task<Warehouse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Warehouse?> GetByCodeAsync(WarehouseCode warehouseCode, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Warehouse>> ListAsync(int skip, int take, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Warehouse warehouse, CancellationToken cancellationToken = default);
}
