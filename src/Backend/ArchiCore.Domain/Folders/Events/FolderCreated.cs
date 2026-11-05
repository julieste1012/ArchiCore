using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.Folders.Events;

public sealed class FolderCreated : DomainEvent
{
    public Guid FolderId { get; }
    public string FolderCode { get; }
    public Guid ArchiveId { get; }
    public Guid ExpedientId { get; }

    public FolderCreated(Guid folderId, string folderCode, Guid archiveId, Guid expedientId)
    {
        FolderId = folderId;
        FolderCode = folderCode;
        ArchiveId = archiveId;
        ExpedientId = expedientId;
    }
}
