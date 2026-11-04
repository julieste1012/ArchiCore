using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.DocumentaryReception.Events;

public sealed class DocumentaryReceptionCreated : DomainEvent
{
    public Guid ReceptionId { get; }
    public Guid ContractId { get; }
    public Guid CustomerId { get; }
    public Guid ReceivedByUserId { get; }

    public DocumentaryReceptionCreated(Guid receptionId, Guid contractId, Guid customerId, Guid receivedByUserId)
    {
        ReceptionId = receptionId;
        ContractId = contractId;
        CustomerId = customerId;
        ReceivedByUserId = receivedByUserId;
    }
}
