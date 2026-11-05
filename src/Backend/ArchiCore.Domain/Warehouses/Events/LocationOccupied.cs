using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.Warehouses.Events;

public sealed class LocationOccupied : DomainEvent
{
    public Guid WarehouseId { get; }
    public Guid LocationId { get; }
    public Guid BoxId { get; }

    public LocationOccupied(Guid warehouseId, Guid locationId, Guid boxId)
    {
        WarehouseId = warehouseId;
        LocationId = locationId;
        BoxId = boxId;
    }
}
