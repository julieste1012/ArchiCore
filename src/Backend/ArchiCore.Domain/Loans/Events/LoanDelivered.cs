using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.Loans.Events;

public sealed class LoanDelivered : DomainEvent
{
    public Guid LoanId { get; }
    public Guid FolderId { get; }
    public Guid DeliveredByUserId { get; }
    public DateTimeOffset DeliveredAtUtc { get; }

    public LoanDelivered(Guid loanId, Guid folderId, Guid deliveredByUserId, DateTimeOffset deliveredAtUtc)
    {
        LoanId = loanId;
        FolderId = folderId;
        DeliveredByUserId = deliveredByUserId;
        DeliveredAtUtc = deliveredAtUtc;
    }
}
