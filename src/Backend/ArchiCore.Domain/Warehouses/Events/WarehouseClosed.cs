using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.Warehouses.Events;

/// <summary>
/// Evento de dominio generado cuando un <see cref="Warehouse"/> es cerrado.
/// </summary>
public sealed class WarehouseClosed : DomainEvent
{
    public Guid WarehouseId { get; }

    public WarehouseClosed(Guid warehouseId)
    {
        WarehouseId = warehouseId;
    }
}
