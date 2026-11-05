using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.Boxes.Events;

public sealed class BoxLocationChanged : DomainEvent
{
    public Guid BoxId { get; }
    public Guid PreviousLocationId { get; }
    public Guid NewLocationId { get; }

    public BoxLocationChanged(Guid boxId, Guid previousLocationId, Guid newLocationId)
    {
        BoxId = boxId;
        PreviousLocationId = previousLocationId;
        NewLocationId = newLocationId;
    }
}
