using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.Boxes.Events;

public sealed class BoxMadeAvailable : DomainEvent
{
    public Guid BoxId { get; }

    public BoxMadeAvailable(Guid boxId) => BoxId = boxId;
}
