using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.Digitization.Events;

public sealed class DigitizationRequested : DomainEvent
{
    public Guid RequestId { get; }
    public Guid ProjectId { get; }
    public Guid FolderId { get; }
    public Guid RequestedByUserId { get; }

    public DigitizationRequested(Guid requestId, Guid projectId, Guid folderId, Guid requestedByUserId)
    {
        RequestId = requestId;
        ProjectId = projectId;
        FolderId = folderId;
        RequestedByUserId = requestedByUserId;
    }
}
