namespace ArchiCore.Domain.Projects;

/// <summary>
/// Representa la asignación de una bodega a un proyecto durante un período de vigencia.
/// </summary>
public sealed class ProjectWarehouseAssignment
{
    /// <summary>
    /// Referencia al Aggregate Warehouse asignado.
    /// </summary>
    public Guid WarehouseId { get; }

    /// <summary>
    /// Fecha de inicio de vigencia de la asignación.
    /// </summary>
    public DateTime AssignedFrom { get; }

    /// <summary>
    /// Fecha en la que la asignación fue liberada. Nulo mientras la asignación esté activa.
    /// </summary>
    public DateTime? ReleasedOn { get; private set; }

    /// <summary>
    /// Indica si la asignación se encuentra actualmente activa.
    /// </summary>
    public bool IsActive => ReleasedOn is null;

    internal ProjectWarehouseAssignment(Guid warehouseId, DateTime assignedFrom)
    {
        WarehouseId = warehouseId;
        AssignedFrom = assignedFrom;
    }

    internal void Release(DateTime releasedOn)
    {
        ReleasedOn = releasedOn;
    }
}
