using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.Folders.Events;

public sealed class AnnexAdded : DomainEvent
{
    public Guid FolderId { get; }
    public Guid DocumentId { get; }
    public int Sequence { get; }
    public string Title { get; }

    public AnnexAdded(Guid folderId, Guid documentId, int sequence, string title)
    {
        FolderId = folderId;
        DocumentId = documentId;
        Sequence = sequence;
        Title = title;
    }
}
