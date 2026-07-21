using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.Projects.Events;

/// <summary>
/// Evento de dominio generado cuando se asigna una bodega a un <see cref="Project"/>.
/// </summary>
public sealed class WarehouseAssigned : DomainEvent
{
    public Guid ProjectId { get; }
    public Guid WarehouseId { get; }
    public DateTime AssignedFrom { get; }

    public WarehouseAssigned(Guid projectId, Guid warehouseId, DateTime assignedFrom)
    {
        ProjectId = projectId;
        WarehouseId = warehouseId;
        AssignedFrom = assignedFrom;
    }
}
