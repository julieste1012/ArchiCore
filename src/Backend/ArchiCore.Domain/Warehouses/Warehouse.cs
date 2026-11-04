using ArchiCore.Domain.Common;
using ArchiCore.Domain.ValueObjects;
using ArchiCore.Domain.Warehouses.Events;

namespace ArchiCore.Domain.Warehouses;

public sealed class Warehouse : Entity, IAggregateRoot
{
    private readonly List<WarehouseLocation> _locations = new();

    public WarehouseCode WarehouseCode { get; private set; }
    public WarehouseStatus Status { get; private set; }
    public IReadOnlyCollection<WarehouseLocation> Locations => _locations.AsReadOnly();

    private Warehouse() => WarehouseCode = null!;

    private Warehouse(WarehouseCode warehouseCode)
    {
        WarehouseCode = warehouseCode;
        Status = WarehouseStatus.Available;
    }

    public static Result<Warehouse> Create(WarehouseCode warehouseCode)
    {
        if (warehouseCode is null)
            return Result.Failure<Warehouse>(new Error("Warehouse.WarehouseCodeRequired", "El código de la bodega es obligatorio."));

        var warehouse = new Warehouse(warehouseCode);
        warehouse.AddDomainEvent(new WarehouseCreated(warehouse.Id, warehouse.WarehouseCode.Value));
        return Result.Success(warehouse);
    }

    public Result Activate()
    {
        var validation = EnsureStatusIs(WarehouseStatus.Available, "Warehouse.InvalidActivation", "La bodega solo puede activarse desde el estado Available.");
        if (validation.IsFailure) return validation;
        Status = WarehouseStatus.Operational;
        MarkAsUpdated();
        AddDomainEvent(new WarehouseActivated(Id));
        return Result.Success();
    }

    public Result StartMaintenance()
    {
        var validation = EnsureStatusIs(WarehouseStatus.Operational, "Warehouse.InvalidMaintenance", "La bodega solo puede iniciar mantenimiento desde el estado Operational.");
        if (validation.IsFailure) return validation;
        Status = WarehouseStatus.Maintenance;
        MarkAsUpdated();
        return Result.Success();
    }

    public Result FinishMaintenance()
    {
        var validation = EnsureStatusIs(WarehouseStatus.Maintenance, "Warehouse.InvalidMaintenance", "La bodega solo puede finalizar mantenimiento desde el estado Maintenance.");
        if (validation.IsFailure) return validation;
        Status = WarehouseStatus.Operational;
        MarkAsUpdated();
        return Result.Success();
    }

    public Result Close()
    {
        var validation = EnsureStatusIs(WarehouseStatus.Maintenance, "Warehouse.InvalidClose", "La bodega solo puede cerrarse desde el estado Maintenance.");
        if (validation.IsFailure) return validation;
        if (_locations.Any(location => location.OccupiedCapacity > 0))
            return Result.Failure(new Error("Warehouse.OccupiedLocations", "No se puede cerrar una bodega mientras existan cajas almacenadas en sus ubicaciones."));
        Status = WarehouseStatus.Closed;
        MarkAsUpdated();
        AddDomainEvent(new WarehouseClosed(Id));
        return Result.Success();
    }

    public Result Archive()
    {
        var validation = EnsureStatusIs(WarehouseStatus.Closed, "Warehouse.InvalidArchive", "La bodega solo puede archivarse desde el estado Closed.");
        if (validation.IsFailure) return validation;
        if (_locations.Any(location => location.OccupiedCapacity > 0))
            return Result.Failure(new Error("Warehouse.OccupiedLocations", "No se puede archivar una bodega mientras existan cajas almacenadas en sus ubicaciones."));
        Status = WarehouseStatus.Archived;
        MarkAsUpdated();
        return Result.Success();
    }

    /// <summary>Crea una ubicación hija siguiendo Aisle → Shelf → Cabinet → Position.</summary>
    public Result<Guid> AddLocation(string code, WarehouseLocationType type, Guid? parentLocationId = null, int? capacity = null)
    {
        if (Status is WarehouseStatus.Maintenance or WarehouseStatus.Closed or WarehouseStatus.Archived)
            return Result.Failure<Guid>(new Error("Warehouse.LocationStructureLocked", "No se puede modificar la estructura de ubicaciones durante mantenimiento o después del cierre."));
        if (string.IsNullOrWhiteSpace(code) || code.Trim().Length > 50)
            return Result.Failure<Guid>(new Error("Warehouse.InvalidLocationCode", "El código de ubicación es obligatorio y no puede superar 50 caracteres."));

        var normalizedCode = code.Trim().ToUpperInvariant();
        if (_locations.Any(location => string.Equals(location.Code, normalizedCode, StringComparison.OrdinalIgnoreCase)))
            return Result.Failure<Guid>(new Error("Warehouse.DuplicateLocationCode", "El código de ubicación ya existe dentro de esta bodega."));
        if (!Enum.IsDefined(type))
            return Result.Failure<Guid>(new Error("Warehouse.InvalidLocationType", "El tipo de ubicación no es válido."));

        if (type == WarehouseLocationType.Aisle)
        {
            if (parentLocationId is not null)
                return Result.Failure<Guid>(new Error("Warehouse.InvalidLocationHierarchy", "Un pasillo no puede tener una ubicación padre."));
        }
        else
        {
            if (parentLocationId is null)
                return Result.Failure<Guid>(new Error("Warehouse.LocationParentRequired", "La ubicación requiere un padre dentro de la misma bodega."));
            var parent = _locations.FirstOrDefault(location => location.Id == parentLocationId.Value);
            var requiredParentType = type switch
            {
                WarehouseLocationType.Shelf => WarehouseLocationType.Aisle,
                WarehouseLocationType.Cabinet => WarehouseLocationType.Shelf,
                WarehouseLocationType.Position => WarehouseLocationType.Cabinet,
                _ => (WarehouseLocationType?)null
            };
            if (parent is null || parent.Type != requiredParentType)
                return Result.Failure<Guid>(new Error("Warehouse.InvalidLocationHierarchy", "La ubicación padre no existe o no corresponde al nivel inmediatamente superior."));
        }

        if (type == WarehouseLocationType.Position)
        {
            if (capacity is null or <= 0)
                return Result.Failure<Guid>(new Error("Warehouse.InvalidLocationCapacity", "Una posición debe tener una capacidad positiva de cajas."));
        }
        else if (capacity is not null)
        {
            return Result.Failure<Guid>(new Error("Warehouse.InvalidLocationCapacity", "La capacidad de cajas solo se configura en ubicaciones de tipo Position."));
        }

        var location = new WarehouseLocation(normalizedCode, type, parentLocationId, capacity);
        _locations.Add(location);
        MarkAsUpdated();
        AddDomainEvent(new LocationCreated(Id, location.Id, location.Code, type, parentLocationId, capacity));
        return Result.Success(location.Id);
    }

    public Result OccupyLocation(Guid locationId, Guid boxId)
    {
        if (Status != WarehouseStatus.Operational)
            return Result.Failure(new Error("Warehouse.NotOperational", "Solo una bodega Operational puede recibir cajas."));
        if (locationId == Guid.Empty || boxId == Guid.Empty)
            return Result.Failure(new Error("Warehouse.LocationAndBoxRequired", "Los identificadores de ubicación y caja son obligatorios."));
        var location = _locations.FirstOrDefault(item => item.Id == locationId);
        if (location is null)
            return Result.Failure(new Error("Warehouse.LocationNotFound", "La ubicación indicada no pertenece a esta bodega."));
        if (_locations.Any(item => item.ContainsBox(boxId)))
            return Result.Failure(new Error("Warehouse.BoxAlreadyStored", "La caja ya ocupa una ubicación dentro de esta bodega."));

        var result = location.Occupy(boxId);
        if (result.IsFailure) return result;
        MarkAsUpdated();
        AddDomainEvent(new LocationOccupied(Id, locationId, boxId));
        return Result.Success();
    }

    public Result ReleaseLocation(Guid locationId, Guid boxId)
    {
        if (locationId == Guid.Empty || boxId == Guid.Empty)
            return Result.Failure(new Error("Warehouse.LocationAndBoxRequired", "Los identificadores de ubicación y caja son obligatorios."));
        var location = _locations.FirstOrDefault(item => item.Id == locationId);
        if (location is null)
            return Result.Failure(new Error("Warehouse.LocationNotFound", "La ubicación indicada no pertenece a esta bodega."));
        var result = location.Release(boxId);
        if (result.IsFailure) return result;
        MarkAsUpdated();
        AddDomainEvent(new LocationReleased(Id, locationId, boxId));
        return Result.Success();
    }

    /// <summary>Traslada una caja entre dos posiciones de la misma bodega.</summary>
    public Result MoveBox(Guid boxId, Guid destinationLocationId)
    {
        if (Status != WarehouseStatus.Operational)
            return Result.Failure(new Error("Warehouse.NotOperational", "Solo una bodega Operational puede mover cajas."));
        if (boxId == Guid.Empty || destinationLocationId == Guid.Empty)
            return Result.Failure(new Error("Warehouse.BoxAndDestinationRequired", "La caja y la ubicación de destino son obligatorias."));

        var source = _locations.FirstOrDefault(location => location.ContainsBox(boxId));
        if (source is null)
            return Result.Failure(new Error("Warehouse.BoxNotStored", "La caja no está almacenada en esta bodega."));
        var destination = _locations.FirstOrDefault(location => location.Id == destinationLocationId);
        if (destination is null)
            return Result.Failure(new Error("Warehouse.LocationNotFound", "La ubicación de destino no pertenece a esta bodega."));
        if (source.Id == destination.Id)
            return Result.Failure(new Error("Warehouse.SameLocation", "La caja ya está almacenada en la ubicación indicada."));

        var destinationValidation = destination.CanOccupy(boxId);
        if (destinationValidation.IsFailure) return destinationValidation;
        var releaseResult = source.Release(boxId);
        if (releaseResult.IsFailure) return releaseResult;
        var occupyResult = destination.Occupy(boxId);
        if (occupyResult.IsFailure)
        {
            source.Occupy(boxId);
            return occupyResult;
        }

        MarkAsUpdated();
        AddDomainEvent(new LocationReleased(Id, source.Id, boxId));
        AddDomainEvent(new LocationOccupied(Id, destination.Id, boxId));
        AddDomainEvent(new BoxRelocated(Id, boxId, source.Id, destination.Id));
        return Result.Success();
    }

    public Result<int> GetLocationOccupancy(Guid locationId)
    {
        var location = _locations.FirstOrDefault(item => item.Id == locationId);
        return location is null
            ? Result.Failure<int>(new Error("Warehouse.LocationNotFound", "La ubicación indicada no pertenece a esta bodega."))
            : Result.Success(location.OccupiedCapacity);
    }

    private Result EnsureStatusIs(WarehouseStatus expected, string errorCode, string errorMessage)
        => Status != expected ? Result.Failure(new Error(errorCode, errorMessage)) : Result.Success();
}
