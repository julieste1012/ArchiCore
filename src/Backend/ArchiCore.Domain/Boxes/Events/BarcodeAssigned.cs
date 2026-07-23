using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.Boxes.Events;

/// <summary>
/// Evento de dominio generado cuando se asigna un código de barras a una <see cref="Box"/>.
/// </summary>
public sealed class BarcodeAssigned : DomainEvent
{
    public Guid BoxId { get; }
    public string Barcode { get; }

    public BarcodeAssigned(Guid boxId, string barcode)
    {
        BoxId = boxId;
        Barcode = barcode;
    }
}
