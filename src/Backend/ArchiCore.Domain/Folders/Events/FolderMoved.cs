using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.Folders.Events;

public sealed class FolderMoved : DomainEvent
{
    public Guid FolderId { get; }
    public Guid PreviousBoxId { get; }
    public Guid NewBoxId { get; }

    public FolderMoved(Guid folderId, Guid previousBoxId, Guid newBoxId)
    {
        FolderId = folderId;
        PreviousBoxId = previousBoxId;
        NewBoxId = newBoxId;
    }
}
