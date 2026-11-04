using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.Loans.Events;

public sealed class LoanOverdue : DomainEvent
{
    public Guid LoanId { get; }
    public Guid FolderId { get; }
    public DateTimeOffset DueAtUtc { get; }
    public DateTimeOffset DetectedAtUtc { get; }

    public LoanOverdue(Guid loanId, Guid folderId, DateTimeOffset dueAtUtc, DateTimeOffset detectedAtUtc)
    {
        LoanId = loanId;
        FolderId = folderId;
        DueAtUtc = dueAtUtc;
        DetectedAtUtc = detectedAtUtc;
    }
}
