using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.Warehouses.Events;

public sealed class LocationCreated : DomainEvent
{
    public Guid WarehouseId { get; }
    public Guid LocationId { get; }
    public string Code { get; }
    public WarehouseLocationType Type { get; }
    public Guid? ParentLocationId { get; }
    public int? Capacity { get; }

    public LocationCreated(Guid warehouseId, Guid locationId, string code, WarehouseLocationType type, Guid? parentLocationId, int? capacity)
    {
        WarehouseId = warehouseId;
        LocationId = locationId;
        Code = code;
        Type = type;
        ParentLocationId = parentLocationId;
        Capacity = capacity;
    }
}
