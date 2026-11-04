using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.Folders.Events;

public sealed class FolderReceived : DomainEvent
{
    public Guid FolderId { get; }
    public FolderReceived(Guid folderId) => FolderId = folderId;
}
