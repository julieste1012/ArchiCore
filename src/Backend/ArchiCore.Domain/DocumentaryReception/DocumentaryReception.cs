using ArchiCore.Domain.Common;
using ArchiCore.Domain.DocumentaryReception.Events;

namespace ArchiCore.Domain.DocumentaryReception;

/// <summary>
/// Operational record for the BP-001 reception and BP-002 counting workflow.
/// It records reception/count data without pretending to model unresolved Archive/Folder/Box
/// aggregate boundaries, document metadata or physical warehouse capacity.
/// </summary>
public sealed class DocumentaryReception : Entity, IAggregateRoot
{
    public Guid ContractId { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid ReceivedByUserId { get; private set; }
    public DateTime ReceivedAtUtc { get; private set; }
    public string? InitialObservations { get; private set; }
    public DocumentaryReceptionStatus Status { get; private set; }

    public Guid? InspectedByUserId { get; private set; }
    public DateTime? InspectionCompletedAtUtc { get; private set; }
    public bool? SafeToAccept { get; private set; }
    public string? InspectionObservations { get; private set; }

    public int? DeclaredBoxCount { get; private set; }
    public int? DeclaredFolderCount { get; private set; }
    public int? CountedBoxCount { get; private set; }
    public int? CountedFolderCount { get; private set; }
    public string? DiscrepancyReport { get; private set; }
    public Guid? CountedByUserId { get; private set; }
    public DateTime? CountedAtUtc { get; private set; }

    public Guid? ApprovedByUserId { get; private set; }
    public DateTime? ApprovedAtUtc { get; private set; }

    public Guid? SuspendedByUserId { get; private set; }
    public DateTime? SuspendedAtUtc { get; private set; }
    public string? SuspensionReason { get; private set; }

    private DocumentaryReception()
    {
        InitialObservations = null;
        InspectionObservations = null;
        DiscrepancyReport = null;
        SuspensionReason = null;
    }

    private DocumentaryReception(
        Guid contractId,
        Guid customerId,
        Guid receivedByUserId,
        string? initialObservations)
    {
        ContractId = contractId;
        CustomerId = customerId;
        ReceivedByUserId = receivedByUserId;
        ReceivedAtUtc = DateTime.UtcNow;
        InitialObservations = NormalizeOptional(initialObservations);
        Status = DocumentaryReceptionStatus.Received;
    }

    public static Result<DocumentaryReception> Create(
        Guid contractId,
        Guid customerId,
        Guid receivedByUserId,
        string? initialObservations)
    {
        if (contractId == Guid.Empty)
            return Result.Failure<DocumentaryReception>(
                new Error("Reception.ContractRequired", "An active contract is required for documentary reception."));

        if (customerId == Guid.Empty)
            return Result.Failure<DocumentaryReception>(
                new Error("Reception.CustomerRequired", "A customer is required for documentary reception."));

        if (receivedByUserId == Guid.Empty)
            return Result.Failure<DocumentaryReception>(
                new Error("Reception.ReceivedByRequired", "The authenticated receiving user is required."));

        if (initialObservations?.Length > 2000)
            return Result.Failure<DocumentaryReception>(
                new Error("Reception.ObservationsTooLong", "Initial observations cannot exceed 2000 characters."));

        var reception = new DocumentaryReception(contractId, customerId, receivedByUserId, initialObservations);
        reception.AddDomainEvent(new DocumentaryReceptionCreated(
            reception.Id, contractId, customerId, receivedByUserId));
        return Result.Success(reception);
    }

    public Result RecordInspection(Guid inspectedByUserId, bool safeToAccept, string? observations)
    {
        if (Status != DocumentaryReceptionStatus.Received)
            return Result.Failure(new Error(
                "Reception.InvalidInspectionState", "Inspection can only be recorded for a newly received delivery."));

        if (inspectedByUserId == Guid.Empty)
            return Result.Failure(new Error("Reception.InspectorRequired", "The authenticated inspector is required."));

        if (observations?.Length > 2000)
            return Result.Failure(new Error("Reception.InspectionTooLong", "Inspection observations cannot exceed 2000 characters."));

        if (!safeToAccept && string.IsNullOrWhiteSpace(observations))
            return Result.Failure(new Error(
                "Reception.IncidentDescriptionRequired",
                "Describe the physical damage or incident when the delivery cannot be safely accepted."));

        InspectedByUserId = inspectedByUserId;
        InspectionCompletedAtUtc = DateTime.UtcNow;
        SafeToAccept = safeToAccept;
        InspectionObservations = NormalizeOptional(observations);
        MarkAsUpdated();
        AddDomainEvent(new DocumentaryReceptionInspectionCompleted(Id, inspectedByUserId, safeToAccept));

        if (safeToAccept)
        {
            Status = DocumentaryReceptionStatus.Inspected;
            return Result.Success();
        }

        var suspensionReason = observations!.Trim();
        Status = DocumentaryReceptionStatus.Suspended;
        SuspendedByUserId = inspectedByUserId;
        SuspendedAtUtc = InspectionCompletedAtUtc;
        SuspensionReason = suspensionReason;
        AddDomainEvent(new DocumentaryReceptionSuspended(Id, inspectedByUserId, suspensionReason));
        return Result.Success();
    }

    public Result RecordCounting(
        Guid countedByUserId,
        int declaredBoxCount,
        int declaredFolderCount,
        int countedBoxCount,
        int countedFolderCount,
        string? discrepancyReport)
    {
        if (Status != DocumentaryReceptionStatus.Inspected)
            return Result.Failure(new Error(
                "Reception.InvalidCountingState", "Counting requires a completed and accepted physical inspection."));

        if (countedByUserId == Guid.Empty)
            return Result.Failure(new Error("Reception.CountedByRequired", "The authenticated counting user is required."));

        if (declaredBoxCount < 0 || declaredFolderCount < 0 || countedBoxCount < 0 || countedFolderCount < 0)
            return Result.Failure(new Error("Reception.InvalidCount", "Documentary counts cannot be negative."));

        var hasDiscrepancies = declaredBoxCount != countedBoxCount
            || declaredFolderCount != countedFolderCount;

        if (hasDiscrepancies && string.IsNullOrWhiteSpace(discrepancyReport))
            return Result.Failure(new Error(
                "Reception.DiscrepancyReportRequired",
                "Record the discrepancies between the delivery inventory and physical counting."));

        if (discrepancyReport?.Length > 4000)
            return Result.Failure(new Error("Reception.DiscrepancyReportTooLong", "The discrepancy report cannot exceed 4000 characters."));

        DeclaredBoxCount = declaredBoxCount;
        DeclaredFolderCount = declaredFolderCount;
        CountedBoxCount = countedBoxCount;
        CountedFolderCount = countedFolderCount;
        DiscrepancyReport = NormalizeOptional(discrepancyReport);
        CountedByUserId = countedByUserId;
        CountedAtUtc = DateTime.UtcNow;
        Status = DocumentaryReceptionStatus.CountingRecorded;
        MarkAsUpdated();

        AddDomainEvent(new DocumentaryReceptionCountingRecorded(
            Id,
            countedByUserId,
            declaredBoxCount,
            declaredFolderCount,
            countedBoxCount,
            countedFolderCount,
            hasDiscrepancies));
        return Result.Success();
    }

    public Result ApproveCounting(Guid approvedByUserId)
    {
        if (Status != DocumentaryReceptionStatus.CountingRecorded)
            return Result.Failure(new Error(
                "Reception.InvalidApprovalState", "Only a recorded documentary count can be approved."));

        if (approvedByUserId == Guid.Empty)
            return Result.Failure(new Error("Reception.ApproverRequired", "The authenticated coordinator is required."));

        ApprovedByUserId = approvedByUserId;
        ApprovedAtUtc = DateTime.UtcNow;
        Status = DocumentaryReceptionStatus.Counted;
        MarkAsUpdated();
        AddDomainEvent(new DocumentaryReceptionCountingApproved(Id, approvedByUserId));
        return Result.Success();
    }

    public Result Suspend(Guid suspendedByUserId, string reason)
    {
        if (Status is DocumentaryReceptionStatus.Counted or DocumentaryReceptionStatus.Suspended)
            return Result.Failure(new Error(
                "Reception.InvalidSuspensionState", "A counted or already suspended reception cannot be suspended again."));

        if (suspendedByUserId == Guid.Empty)
            return Result.Failure(new Error("Reception.SuspendedByRequired", "The authenticated coordinator is required."));

        if (string.IsNullOrWhiteSpace(reason) || reason.Trim().Length > 1000)
            return Result.Failure(new Error(
                "Reception.InvalidSuspensionReason", "A suspension reason between 1 and 1000 characters is required."));

        var suspensionReason = reason.Trim();
        SuspendedByUserId = suspendedByUserId;
        SuspendedAtUtc = DateTime.UtcNow;
        SuspensionReason = suspensionReason;
        Status = DocumentaryReceptionStatus.Suspended;
        MarkAsUpdated();
        AddDomainEvent(new DocumentaryReceptionSuspended(Id, suspendedByUserId, suspensionReason));
        return Result.Success();
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
