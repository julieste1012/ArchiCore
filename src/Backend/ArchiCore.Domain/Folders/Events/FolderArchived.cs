using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.Folders.Events;

public sealed class FolderArchived : DomainEvent
{
    public Guid FolderId { get; }
    public FolderArchived(Guid folderId) => FolderId = folderId;
}
