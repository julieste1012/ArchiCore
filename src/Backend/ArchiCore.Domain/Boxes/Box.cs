using ArchiCore.Domain.Boxes.Events;
using ArchiCore.Domain.Common;
using ArchiCore.Domain.ValueObjects;

namespace ArchiCore.Domain.Boxes;

/// <summary>
/// Aggregate Root que representa el contenedor físico de carpetas documentales.
/// </summary>
public sealed class Box : Entity, IAggregateRoot
{
    private readonly List<BoxFolderReference> _folderReferences = new();

    public BoxCode BoxCode { get; private set; }
    public Barcode? Barcode { get; private set; }
    public BoxStatus Status { get; private set; }
    public Guid? WarehouseLocationId { get; private set; }
    public IReadOnlyCollection<BoxFolderReference> FolderReferences => _folderReferences.AsReadOnly();
    public IReadOnlyCollection<Guid> FolderIds => _folderReferences.Select(item => item.FolderId).ToArray();

    private Box(BoxCode boxCode)
    {
        BoxCode = boxCode;
        Status = BoxStatus.Created;
    }

    private Box()
    {
        BoxCode = null!;
    }

    public static Result<Box> Create(BoxCode boxCode)
    {
        if (boxCode is null)
            return Result.Failure<Box>(new Error("Box.BoxCodeRequired", "El código de la caja es obligatorio."));

        var box = new Box(boxCode);
        box.AddDomainEvent(new BoxCreated(box.Id, box.BoxCode.Value));
        return Result.Success(box);
    }

    public Result AssignBarcode(Barcode barcode)
    {
        if (Status != BoxStatus.Created)
            return Result.Failure(new Error("Box.InvalidBarcodeAssignmentState", "El código de barras solo puede asignarse mientras la caja está en estado Created."));
        if (barcode is null)
            return Result.Failure(new Error("Box.BarcodeRequired", "El código de barras es obligatorio."));
        if (Barcode is not null)
            return Result.Failure(new Error("Box.BarcodeAlreadyAssigned", "La caja ya tiene un código de barras asignado."));

        Barcode = barcode;
        MarkAsUpdated();
        AddDomainEvent(new BarcodeAssigned(Id, barcode.Value));
        return Result.Success();
    }

    /// <summary>Created -> Available. Requiere que la caja tenga su barcode físico.</summary>
    public Result MakeAvailable()
    {
        var validation = EnsureStatusIs(BoxStatus.Created, "Box.InvalidAvailabilityTransition", "Solo una caja Created puede pasar a Available.");
        if (validation.IsFailure) return validation;
        if (Barcode is null)
            return Result.Failure(new Error("Box.BarcodeRequired", "La caja debe tener un código de barras antes de estar disponible."));

        Status = BoxStatus.Available;
        MarkAsUpdated();
        AddDomainEvent(new BoxMadeAvailable(Id));
        return Result.Success();
    }

    /// <summary>Available -> InStorage. La aplicación valida que la ubicación pertenezca a la bodega operativa.</summary>
    public Result Store(Guid warehouseLocationId)
    {
        var validation = EnsureStatusIs(BoxStatus.Available, "Box.InvalidStorageTransition", "Solo una caja Available puede ingresar a almacenamiento.");
        if (validation.IsFailure) return validation;
        if (warehouseLocationId == Guid.Empty)
            return Result.Failure(new Error("Box.WarehouseLocationRequired", "La ubicación física de almacenamiento es obligatoria."));

        WarehouseLocationId = warehouseLocationId;
        Status = BoxStatus.InStorage;
        MarkAsUpdated();
        AddDomainEvent(new BoxStored(Id, warehouseLocationId));
        return Result.Success();
    }

    /// <summary>InStorage -> InTransit. La aplicación debe liberar la posición en Warehouse dentro de la misma transacción.</summary>
    public Result Move()
    {
        var validation = EnsureStatusIs(BoxStatus.InStorage, "Box.InvalidMove", "La caja solo puede salir de almacenamiento desde el estado InStorage.");
        if (validation.IsFailure) return validation;

        var sourceLocationId = WarehouseLocationId;
        WarehouseLocationId = null;
        Status = BoxStatus.InTransit;
        MarkAsUpdated();
        AddDomainEvent(new BoxMoved(Id, sourceLocationId));
        return Result.Success();
    }

    /// <summary>InTransit -> LoanProcess.</summary>
    public Result StartLoanProcess()
    {
        var validation = EnsureStatusIs(BoxStatus.InTransit, "Box.InvalidLoanProcess", "El proceso de préstamo solo puede comenzar desde el estado InTransit.");
        if (validation.IsFailure) return validation;

        Status = BoxStatus.LoanProcess;
        MarkAsUpdated();
        AddDomainEvent(new BoxLoanProcessStarted(Id));
        return Result.Success();
    }

    public Result AddFolder(Guid folderId)
    {
        if (Status != BoxStatus.InStorage)
            return Result.Failure(new Error("Box.InvalidFolderAssignmentState", "Las carpetas solo pueden asociarse a una caja almacenada."));
        if (folderId == Guid.Empty)
            return Result.Failure(new Error("Box.FolderIdRequired", "El identificador de la carpeta es obligatorio."));
        if (_folderReferences.Any(item => item.FolderId == folderId))
            return Result.Failure(new Error("Box.FolderAlreadyAssigned", "La carpeta ya está asociada a esta caja."));

        _folderReferences.Add(new BoxFolderReference(folderId));
        MarkAsUpdated();
        AddDomainEvent(new FolderAdded(Id, folderId));
        return Result.Success();
    }

    public Result RemoveFolder(Guid folderId)
    {
        if (Status != BoxStatus.InStorage)
            return Result.Failure(new Error("Box.InvalidFolderRemovalState", "Las carpetas solo pueden retirarse de una caja almacenada."));
        if (_folderReferences.RemoveAll(item => item.FolderId == folderId) == 0)
            return Result.Failure(new Error("Box.FolderNotFound", "La carpeta no está asociada a esta caja."));

        MarkAsUpdated();
        AddDomainEvent(new FolderRemoved(Id, folderId));
        return Result.Success();
    }

    public Result Return()
    {
        var validation = EnsureStatusIs(BoxStatus.LoanProcess, "Box.InvalidReturn", "La caja solo puede devolverse desde el estado LoanProcess.");
        if (validation.IsFailure) return validation;

        Status = BoxStatus.Returned;
        MarkAsUpdated();
        AddDomainEvent(new BoxReturned(Id));
        return Result.Success();
    }

    public Result Archive()
    {
        var validation = EnsureStatusIs(BoxStatus.Returned, "Box.InvalidArchive", "La caja solo puede archivarse desde el estado Returned.");
        if (validation.IsFailure) return validation;
        Status = BoxStatus.Archived;
        MarkAsUpdated();
        AddDomainEvent(new BoxArchived(Id));
        return Result.Success();
    }

    private Result EnsureStatusIs(BoxStatus expected, string errorCode, string errorMessage)
        => Status != expected ? Result.Failure(new Error(errorCode, errorMessage)) : Result.Success();
}
