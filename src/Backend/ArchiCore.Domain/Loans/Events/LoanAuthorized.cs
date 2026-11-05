using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.Loans.Events;

public sealed class LoanAuthorized : DomainEvent
{
    public Guid LoanId { get; }
    public Guid FolderId { get; }
    public Guid AuthorizedByUserId { get; }
    public DateTimeOffset AuthorizedAtUtc { get; }

    public LoanAuthorized(Guid loanId, Guid folderId, Guid authorizedByUserId, DateTimeOffset authorizedAtUtc)
    {
        LoanId = loanId;
        FolderId = folderId;
        AuthorizedByUserId = authorizedByUserId;
        AuthorizedAtUtc = authorizedAtUtc;
    }
}
