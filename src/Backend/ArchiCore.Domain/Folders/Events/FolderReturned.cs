using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.Folders.Events;

public sealed class FolderReturned : DomainEvent
{
    public Guid FolderId { get; }
    public Guid LoanId { get; }

    public FolderReturned(Guid folderId, Guid loanId)
    {
        FolderId = folderId;
        LoanId = loanId;
    }
}
