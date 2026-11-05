using ArchiCore.Domain.Common;

namespace ArchiCore.Domain.Warehouses;

/// <summary>Entity interna de Warehouse. Solo Position puede almacenar cajas.</summary>
public sealed class WarehouseLocation : Entity
{
    private readonly List<WarehouseLocationBoxReference> _boxReferences = new();

    public string Code { get; private set; } = string.Empty;
    public WarehouseLocationType Type { get; private set; }
    public Guid? ParentLocationId { get; private set; }
    public int? Capacity { get; private set; }
    public IReadOnlyCollection<WarehouseLocationBoxReference> BoxReferences => _boxReferences.AsReadOnly();

    // EF Core tracks the stable mutable collection through this internal navigation.
    // Callers outside the domain assembly only receive the read-only view above.
    internal ICollection<WarehouseLocationBoxReference> BoxReferencesForPersistence => _boxReferences;
    public IReadOnlyCollection<Guid> BoxIds => _boxReferences.Select(item => item.BoxId).ToArray();
    public int OccupiedCapacity => _boxReferences.Count;

    private WarehouseLocation()
    {
        Code = string.Empty;
    }

    internal WarehouseLocation(string code, WarehouseLocationType type, Guid? parentLocationId, int? capacity)
    {
        Code = code;
        Type = type;
        ParentLocationId = parentLocationId;
        Capacity = capacity;
    }

    internal bool ContainsBox(Guid boxId) => _boxReferences.Any(item => item.BoxId == boxId);

    internal Result CanOccupy(Guid boxId)
    {
        if (Type != WarehouseLocationType.Position)
            return Result.Failure(new Error("Warehouse.LocationCannotStoreBoxes", "Solo una ubicación de tipo Position puede almacenar cajas."));
        if (boxId == Guid.Empty)
            return Result.Failure(new Error("Warehouse.BoxIdRequired", "El identificador de la caja es obligatorio."));
        if (_boxReferences.Any(item => item.BoxId == boxId))
            return Result.Failure(new Error("Warehouse.BoxAlreadyInLocation", "La caja ya está registrada en esta ubicación."));
        if (Capacity is null || _boxReferences.Count >= Capacity.Value)
            return Result.Failure(new Error("Warehouse.LocationCapacityExceeded", "La ubicación no tiene capacidad disponible para otra caja."));
        return Result.Success();
    }

    internal Result Occupy(Guid boxId)
    {
        var validation = CanOccupy(boxId);
        if (validation.IsFailure)
            return validation;
        _boxReferences.Add(new WarehouseLocationBoxReference(boxId));
        MarkAsUpdated();
        return Result.Success();
    }

    internal Result Release(Guid boxId)
    {
        if (_boxReferences.RemoveAll(item => item.BoxId == boxId) == 0)
            return Result.Failure(new Error("Warehouse.BoxNotInLocation", "La caja no está registrada en la ubicación indicada."));
        MarkAsUpdated();
        return Result.Success();
    }
}
