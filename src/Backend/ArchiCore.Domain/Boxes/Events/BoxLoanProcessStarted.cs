using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.Boxes.Events;

public sealed class BoxLoanProcessStarted : DomainEvent
{
    public Guid BoxId { get; }

    public BoxLoanProcessStarted(Guid boxId) => BoxId = boxId;
}
