using ArchiCore.Domain.Boxes.Events;
using ArchiCore.Domain.Common;
using ArchiCore.Domain.ValueObjects;

namespace ArchiCore.Domain.Boxes;

/// <summary>
/// Aggregate Root que representa la unidad física de almacenamiento utilizada
/// por MULTISERTEC para custodiar carpetas documentales.
/// </summary>
public sealed class Box : Entity, IAggregateRoot
{
    /// <summary>
    /// Código de negocio de la caja.
    /// </summary>
    public BoxCode BoxCode { get; private set; }

    /// <summary>
    /// Código de barras físico de la caja.
    /// </summary>
    public Barcode? Barcode { get; private set; }

    /// <summary>
    /// Estado actual de la caja.
    /// </summary>
    public BoxStatus Status { get; private set; }

    private Box(BoxCode boxCode)
    {
        BoxCode = boxCode;
        Status = BoxStatus.Created;
    }

    /// <summary>
    /// Crea una nueva caja en estado <see cref="BoxStatus.Created"/>.
    /// </summary>
    public static Result<Box> Create(BoxCode boxCode)
    {
        if (boxCode is null)
        {
            return Result.Failure<Box>(
                new Error("Box.BoxCodeRequired", "El código de la caja es obligatorio."));
        }

        var box = new Box(boxCode);

        box.AddDomainEvent(new BoxCreated(box.Id, box.BoxCode.Value));

        return Result.Success(box);
    }

    /// <summary>
    /// Asigna el código de barras físico de la caja.
    /// </summary>
    public Result AssignBarcode(Barcode barcode)
    {
        if (barcode is null)
        {
            return Result.Failure(
                new Error("Box.BarcodeRequired", "El código de barras es obligatorio."));
        }

        Barcode = barcode;
        MarkAsUpdated();
        AddDomainEvent(new BarcodeAssigned(Id, Barcode.Value));

        return Result.Success();
    }

    /// <summary>
    /// Reubica la caja desde el estado <see cref="BoxStatus.InStorage"/> hacia <see cref="BoxStatus.InTransit"/>.
    /// </summary>
    public Result Move()
    {
        var validation = EnsureStatusIs(
            BoxStatus.InStorage,
            "Box.InvalidMove",
            "La caja solo puede reubicarse desde el estado InStorage.");

        if (validation.IsFailure)
        {
            return validation;
        }

        Status = BoxStatus.InTransit;
        MarkAsUpdated();
        AddDomainEvent(new BoxMoved(Id));

        return Result.Success();
    }

    /// <summary>
    /// Registra el retorno de la caja desde el estado <see cref="BoxStatus.LoanProcess"/>.
    /// </summary>
    public Result Return()
    {
        var validation = EnsureStatusIs(
            BoxStatus.LoanProcess,
            "Box.InvalidReturn",
            "La caja solo puede devolverse desde el estado LoanProcess.");

        if (validation.IsFailure)
        {
            return validation;
        }

        Status = BoxStatus.Returned;
        MarkAsUpdated();
        AddDomainEvent(new BoxReturned(Id));

        return Result.Success();
    }

    /// <summary>
    /// Archiva la caja desde el estado <see cref="BoxStatus.Returned"/>.
    /// </summary>
    public Result Archive()
    {
        var validation = EnsureStatusIs(
            BoxStatus.Returned,
            "Box.InvalidArchive",
            "La caja solo puede archivarse desde el estado Returned.");

        if (validation.IsFailure)
        {
            return validation;
        }

        Status = BoxStatus.Archived;
        MarkAsUpdated();
        AddDomainEvent(new BoxArchived(Id));

        return Result.Success();
    }

    private Result EnsureStatusIs(BoxStatus expected, string errorCode, string errorMessage)
    {
        if (Status != expected)
        {
            return Result.Failure(new Error(errorCode, errorMessage));
        }

        return Result.Success();
    }
}
