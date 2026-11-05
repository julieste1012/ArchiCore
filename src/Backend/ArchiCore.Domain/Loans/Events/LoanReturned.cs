using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.Loans.Events;

public sealed class LoanReturned : DomainEvent
{
    public Guid LoanId { get; }
    public Guid FolderId { get; }
    public Guid ReturnedByUserId { get; }
    public DateTimeOffset ReturnedAtUtc { get; }

    public LoanReturned(Guid loanId, Guid folderId, Guid returnedByUserId, DateTimeOffset returnedAtUtc)
    {
        LoanId = loanId;
        FolderId = folderId;
        ReturnedByUserId = returnedByUserId;
        ReturnedAtUtc = returnedAtUtc;
    }
}
