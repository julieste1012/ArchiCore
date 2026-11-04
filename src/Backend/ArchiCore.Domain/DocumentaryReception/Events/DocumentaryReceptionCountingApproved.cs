using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.DocumentaryReception.Events;

public sealed class DocumentaryReceptionCountingApproved : DomainEvent
{
    public Guid ReceptionId { get; }
    public Guid ApprovedByUserId { get; }

    public DocumentaryReceptionCountingApproved(Guid receptionId, Guid approvedByUserId)
    {
        ReceptionId = receptionId;
        ApprovedByUserId = approvedByUserId;
    }
}
