using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.Warehouses.Events;

/// <summary>
/// Evento de dominio generado cuando un <see cref="Warehouse"/> es activado.
/// </summary>
public sealed class WarehouseActivated : DomainEvent
{
    public Guid WarehouseId { get; }

    public WarehouseActivated(Guid warehouseId)
    {
        WarehouseId = warehouseId;
    }
}
