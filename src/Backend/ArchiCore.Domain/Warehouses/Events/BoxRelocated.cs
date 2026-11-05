using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.Warehouses.Events;

public sealed class BoxRelocated : DomainEvent
{
    public Guid WarehouseId { get; }
    public Guid BoxId { get; }
    public Guid SourceLocationId { get; }
    public Guid DestinationLocationId { get; }

    public BoxRelocated(Guid warehouseId, Guid boxId, Guid sourceLocationId, Guid destinationLocationId)
    {
        WarehouseId = warehouseId;
        BoxId = boxId;
        SourceLocationId = sourceLocationId;
        DestinationLocationId = destinationLocationId;
    }
}
