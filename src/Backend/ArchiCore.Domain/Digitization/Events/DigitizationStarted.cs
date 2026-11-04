using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.Digitization.Events;

/// <summary>Evento operativo para auditar el inicio efectivo del procesamiento.</summary>
public sealed class DigitizationStarted : DomainEvent
{
    public Guid RequestId { get; }
    public Guid FolderId { get; }
    public Guid StartedByUserId { get; }

    public DigitizationStarted(Guid requestId, Guid folderId, Guid startedByUserId)
    {
        RequestId = requestId;
        FolderId = folderId;
        StartedByUserId = startedByUserId;
    }
}
