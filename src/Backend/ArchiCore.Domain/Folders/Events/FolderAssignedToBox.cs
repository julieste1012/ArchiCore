using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.Folders.Events;

public sealed class FolderAssignedToBox : DomainEvent
{
    public Guid FolderId { get; }
    public Guid BoxId { get; }

    public FolderAssignedToBox(Guid folderId, Guid boxId)
    {
        FolderId = folderId;
        BoxId = boxId;
    }
}
