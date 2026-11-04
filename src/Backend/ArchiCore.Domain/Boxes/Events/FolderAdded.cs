using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.Boxes.Events;

public sealed class FolderAdded : DomainEvent
{
    public Guid BoxId { get; }
    public Guid FolderId { get; }

    public FolderAdded(Guid boxId, Guid folderId)
    {
        BoxId = boxId;
        FolderId = folderId;
    }
}
