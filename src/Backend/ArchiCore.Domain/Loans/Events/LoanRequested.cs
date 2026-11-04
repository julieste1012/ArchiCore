using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.Loans.Events;

public sealed class LoanRequested : DomainEvent
{
    public Guid LoanId { get; }
    public Guid ProjectId { get; }
    public Guid FolderId { get; }
    public Guid RequestedByUserId { get; }
    public DateTimeOffset RequestedAtUtc { get; }
    public DateTimeOffset DueAtUtc { get; }

    public LoanRequested(Guid loanId, Guid projectId, Guid folderId, Guid requestedByUserId, DateTimeOffset requestedAtUtc, DateTimeOffset dueAtUtc)
    {
        LoanId = loanId;
        ProjectId = projectId;
        FolderId = folderId;
        RequestedByUserId = requestedByUserId;
        RequestedAtUtc = requestedAtUtc;
        DueAtUtc = dueAtUtc;
    }
}
