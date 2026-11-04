using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.Folders.Events;

public sealed class DocumentRegistered : DomainEvent
{
    public Guid FolderId { get; }
    public Guid DocumentId { get; }
    public string Title { get; }

    public DocumentRegistered(Guid folderId, Guid documentId, string title)
    {
        FolderId = folderId;
        DocumentId = documentId;
        Title = title;
    }
}
