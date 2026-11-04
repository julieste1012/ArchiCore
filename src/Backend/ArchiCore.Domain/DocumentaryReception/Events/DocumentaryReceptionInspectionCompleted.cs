using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.DocumentaryReception.Events;

public sealed class DocumentaryReceptionInspectionCompleted : DomainEvent
{
    public Guid ReceptionId { get; }
    public Guid InspectedByUserId { get; }
    public bool SafeToAccept { get; }

    public DocumentaryReceptionInspectionCompleted(Guid receptionId, Guid inspectedByUserId, bool safeToAccept)
    {
        ReceptionId = receptionId;
        InspectedByUserId = inspectedByUserId;
        SafeToAccept = safeToAccept;
    }
}
