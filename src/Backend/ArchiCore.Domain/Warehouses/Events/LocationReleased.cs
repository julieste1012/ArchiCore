using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.Warehouses.Events;

public sealed class LocationReleased : DomainEvent
{
    public Guid WarehouseId { get; }
    public Guid LocationId { get; }
    public Guid BoxId { get; }

    public LocationReleased(Guid warehouseId, Guid locationId, Guid boxId)
    {
        WarehouseId = warehouseId;
        LocationId = locationId;
        BoxId = boxId;
    }
}
