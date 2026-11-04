using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.Folders.Events;

public sealed class FolderOrganized : DomainEvent
{
    public Guid FolderId { get; }
    public FolderOrganized(Guid folderId) => FolderId = folderId;
}
