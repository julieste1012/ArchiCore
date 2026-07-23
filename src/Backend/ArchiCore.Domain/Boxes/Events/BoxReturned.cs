using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.Boxes.Events;

/// <summary>
/// Evento de dominio generado cuando una <see cref="Box"/> es devuelta.
/// </summary>
public sealed class BoxReturned : DomainEvent
{
    public Guid BoxId { get; }

    public BoxReturned(Guid boxId)
    {
        BoxId = boxId;
    }
}
