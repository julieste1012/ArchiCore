using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.Archives.Events;

public sealed class ArchiveCreated : DomainEvent
{
    public Guid ArchiveId { get; }
    public Guid ProjectId { get; }

    public ArchiveCreated(Guid archiveId, Guid projectId)
    {
        ArchiveId = archiveId;
        ProjectId = projectId;
    }
}
