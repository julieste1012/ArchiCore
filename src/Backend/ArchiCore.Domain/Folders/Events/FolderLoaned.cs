using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.Folders.Events;

public sealed class FolderLoaned : DomainEvent
{
    public Guid FolderId { get; }
    public Guid LoanId { get; }

    public FolderLoaned(Guid folderId, Guid loanId)
    {
        FolderId = folderId;
        LoanId = loanId;
    }
}
