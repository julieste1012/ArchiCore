using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.Warehouses.Events;

/// <summary>
/// Evento de dominio generado cuando un <see cref="Warehouse"/> es creado.
/// </summary>
public sealed class WarehouseCreated : DomainEvent
{
    public Guid WarehouseId { get; }
    public string WarehouseCode { get; }

    public WarehouseCreated(Guid warehouseId, string warehouseCode)
    {
        WarehouseId = warehouseId;
        WarehouseCode = warehouseCode;
    }
}
