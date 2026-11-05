using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.Digitization.Events;

public sealed class DigitizationCompleted : DomainEvent
{
    public Guid RequestId { get; }
    public Guid FolderId { get; }
    public Guid CompletedByUserId { get; }
    public int GeneratedFileCount { get; }

    public DigitizationCompleted(Guid requestId, Guid folderId, Guid completedByUserId, int generatedFileCount)
    {
        RequestId = requestId;
        FolderId = folderId;
        CompletedByUserId = completedByUserId;
        GeneratedFileCount = generatedFileCount;
    }
}
