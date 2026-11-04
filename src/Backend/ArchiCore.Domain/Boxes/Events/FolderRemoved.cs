using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.Boxes.Events;

public sealed class FolderRemoved : DomainEvent
{
    public Guid BoxId { get; }
    public Guid FolderId { get; }

    public FolderRemoved(Guid boxId, Guid folderId)
    {
        BoxId = boxId;
        FolderId = folderId;
    }
}
