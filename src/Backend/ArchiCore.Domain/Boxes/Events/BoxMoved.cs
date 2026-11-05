using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.Boxes.Events;

/// <summary>Evento generado cuando la caja sale de su ubicación física.</summary>
public sealed class BoxMoved : DomainEvent
{
    public Guid BoxId { get; }
    public Guid? SourceLocationId { get; }

    public BoxMoved(Guid boxId)
        : this(boxId, null)
    {
    }

    public BoxMoved(Guid boxId, Guid? sourceLocationId)
    {
        BoxId = boxId;
        SourceLocationId = sourceLocationId;
    }
}
