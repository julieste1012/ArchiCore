namespace ArchiCore.Domain.Warehouses;

/// <summary>
/// Estados posibles del ciclo de vida de un <see cref="Warehouse"/>.
/// </summary>
public enum WarehouseStatus
{
    Available,
    Operational,
    Maintenance,
    Closed,
    Archived
}
