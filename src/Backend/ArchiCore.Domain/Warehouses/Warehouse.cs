using ArchiCore.Domain.Common;
using ArchiCore.Domain.ValueObjects;
using ArchiCore.Domain.Warehouses.Events;

namespace ArchiCore.Domain.Warehouses;

/// <summary>
/// Aggregate Root que representa una bodega física administrada por MULTISERTEC,
/// destinada al almacenamiento seguro de documentación, equipos y demás elementos
/// asociados a uno o varios proyectos.
/// </summary>
public sealed class Warehouse : Entity, IAggregateRoot
{
    /// <summary>
    /// Código de negocio de la bodega.
    /// </summary>
    public WarehouseCode WarehouseCode { get; private set; }

    /// <summary>
    /// Estado actual de la bodega.
    /// </summary>
    public WarehouseStatus Status { get; private set; }

    private Warehouse(WarehouseCode warehouseCode)
    {
        WarehouseCode = warehouseCode;
        Status = WarehouseStatus.Available;
    }

    /// <summary>
    /// Crea una nueva bodega en estado <see cref="WarehouseStatus.Available"/>.
    /// </summary>
    public static Result<Warehouse> Create(WarehouseCode warehouseCode)
    {
        if (warehouseCode is null)
        {
            return Result.Failure<Warehouse>(
                new Error("Warehouse.WarehouseCodeRequired", "El código de la bodega es obligatorio."));
        }

        var warehouse = new Warehouse(warehouseCode);

        warehouse.AddDomainEvent(new WarehouseCreated(warehouse.Id, warehouse.WarehouseCode.Value));

        return Result.Success(warehouse);
    }

    /// <summary>
    /// Activa la bodega desde el estado <see cref="WarehouseStatus.Available"/>.
    /// </summary>
    public Result Activate()
    {
        var validation = EnsureStatusIs(
            WarehouseStatus.Available,
            "Warehouse.InvalidActivation",
            "La bodega solo puede activarse desde el estado Available.");

        if (validation.IsFailure)
        {
            return validation;
        }

        Status = WarehouseStatus.Operational;
        MarkAsUpdated();
        AddDomainEvent(new WarehouseActivated(Id));

        return Result.Success();
    }

    /// <summary>
    /// Inicia el mantenimiento de la bodega desde el estado <see cref="WarehouseStatus.Operational"/>.
    /// </summary>
    public Result StartMaintenance()
    {
        var validation = EnsureStatusIs(
            WarehouseStatus.Operational,
            "Warehouse.InvalidMaintenance",
            "La bodega solo puede iniciar mantenimiento desde el estado Operational.");

        if (validation.IsFailure)
        {
            return validation;
        }

        Status = WarehouseStatus.Maintenance;
        MarkAsUpdated();

        return Result.Success();
    }

    /// <summary>
    /// Finaliza el mantenimiento de la bodega desde el estado <see cref="WarehouseStatus.Maintenance"/>,
    /// retornando a <see cref="WarehouseStatus.Operational"/>.
    /// </summary>
    public Result FinishMaintenance()
    {
        var validation = EnsureStatusIs(
            WarehouseStatus.Maintenance,
            "Warehouse.InvalidMaintenance",
            "La bodega solo puede finalizar mantenimiento desde el estado Maintenance.");

        if (validation.IsFailure)
        {
            return validation;
        }

        Status = WarehouseStatus.Operational;
        MarkAsUpdated();

        return Result.Success();
    }

    /// <summary>
    /// Cierra la bodega desde el estado <see cref="WarehouseStatus.Maintenance"/>.
    /// </summary>
    public Result Close()
    {
        var validation = EnsureStatusIs(
            WarehouseStatus.Maintenance,
            "Warehouse.InvalidClose",
            "La bodega solo puede cerrarse desde el estado Maintenance.");

        if (validation.IsFailure)
        {
            return validation;
        }

        Status = WarehouseStatus.Closed;
        MarkAsUpdated();
        AddDomainEvent(new WarehouseClosed(Id));

        return Result.Success();
    }

    /// <summary>
    /// Archiva la bodega desde el estado <see cref="WarehouseStatus.Closed"/>.
    /// </summary>
    public Result Archive()
    {
        var validation = EnsureStatusIs(
            WarehouseStatus.Closed,
            "Warehouse.InvalidArchive",
            "La bodega solo puede archivarse desde el estado Closed.");

        if (validation.IsFailure)
        {
            return validation;
        }

        Status = WarehouseStatus.Archived;
        MarkAsUpdated();

        return Result.Success();
    }

    private Result EnsureStatusIs(WarehouseStatus expected, string errorCode, string errorMessage)
    {
        if (Status != expected)
        {
            return Result.Failure(new Error(errorCode, errorMessage));
        }

        return Result.Success();
    }
}
