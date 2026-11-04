namespace ArchiCore.Domain.Warehouses;

/// <summary>Identity reference to a Box occupying this WarehouseLocation.</summary>
public sealed class WarehouseLocationBoxReference
{
    public Guid BoxId { get; private set; }

    private WarehouseLocationBoxReference() { }
    internal WarehouseLocationBoxReference(Guid boxId) => BoxId = boxId;
}
