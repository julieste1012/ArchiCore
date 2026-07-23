using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.Boxes.Events;

/// <summary>
/// Evento de dominio generado cuando una <see cref="Box"/> es reubicada.
/// </summary>
public sealed class BoxMoved : DomainEvent
{
    public Guid BoxId { get; }

    public BoxMoved(Guid boxId)
    {
        BoxId = boxId;
    }
}
