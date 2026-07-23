using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.Boxes.Events;

/// <summary>
/// Evento de dominio generado cuando una <see cref="Box"/> es archivada.
/// </summary>
public sealed class BoxArchived : DomainEvent
{
    public Guid BoxId { get; }

    public BoxArchived(Guid boxId)
    {
        BoxId = boxId;
    }
}
