using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.Boxes.Events;

public sealed class BoxStored : DomainEvent
{
    public Guid BoxId { get; }
    public Guid WarehouseLocationId { get; }

    public BoxStored(Guid boxId, Guid warehouseLocationId)
    {
        BoxId = boxId;
        WarehouseLocationId = warehouseLocationId;
    }
}
