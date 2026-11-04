using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.Folders.Events;

public sealed class FolioAdded : DomainEvent
{
    public Guid FolderId { get; }
    public Guid DocumentId { get; }
    public int Number { get; }

    public FolioAdded(Guid folderId, Guid documentId, int number)
    {
        FolderId = folderId;
        DocumentId = documentId;
        Number = number;
    }
}
