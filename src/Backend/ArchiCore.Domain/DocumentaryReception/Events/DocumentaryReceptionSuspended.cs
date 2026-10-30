using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.DocumentaryReception.Events;

public sealed class DocumentaryReceptionSuspended : DomainEvent
{
    public Guid ReceptionId { get; }
    public Guid SuspendedByUserId { get; }
    public string Reason { get; }

    public DocumentaryReceptionSuspended(Guid receptionId, Guid suspendedByUserId, string reason)
    {
        ReceptionId = receptionId;
        SuspendedByUserId = suspendedByUserId;
        Reason = reason;
    }
}
