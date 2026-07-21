using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.Projects.Events;

/// <summary>
/// Evento de dominio generado cuando se libera la asignación de una bodega a un <see cref="Project"/>.
/// </summary>
public sealed class WarehouseReleased : DomainEvent
{
    public Guid ProjectId { get; }
    public Guid WarehouseId { get; }
    public DateTime ReleasedOn { get; }

    public WarehouseReleased(Guid projectId, Guid warehouseId, DateTime releasedOn)
    {
        ProjectId = projectId;
        WarehouseId = warehouseId;
        ReleasedOn = releasedOn;
    }
}
