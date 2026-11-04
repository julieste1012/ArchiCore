using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.DocumentaryReception.Events;

public sealed class DocumentaryReceptionCountingRecorded : DomainEvent
{
    public Guid ReceptionId { get; }
    public Guid RecordedByUserId { get; }
    public int DeclaredBoxCount { get; }
    public int DeclaredFolderCount { get; }
    public int CountedBoxCount { get; }
    public int CountedFolderCount { get; }
    public bool HasDiscrepancies { get; }

    public DocumentaryReceptionCountingRecorded(
        Guid receptionId,
        Guid recordedByUserId,
        int declaredBoxCount,
        int declaredFolderCount,
        int countedBoxCount,
        int countedFolderCount,
        bool hasDiscrepancies)
    {
        ReceptionId = receptionId;
        RecordedByUserId = recordedByUserId;
        DeclaredBoxCount = declaredBoxCount;
        DeclaredFolderCount = declaredFolderCount;
        CountedBoxCount = countedBoxCount;
        CountedFolderCount = countedFolderCount;
        HasDiscrepancies = hasDiscrepancies;
    }
}
