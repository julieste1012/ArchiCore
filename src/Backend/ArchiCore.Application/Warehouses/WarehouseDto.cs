using ArchiCore.Domain.Warehouses;

namespace ArchiCore.Application.Warehouses;

public sealed record WarehouseDto(
    Guid Id,
    string WarehouseCode,
    WarehouseStatus Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record CreateWarehouseRequest(string WarehouseCode);
