using ArchiCore.Domain.DocumentaryReception;

namespace ArchiCore.Application.Receptions;

public sealed record DocumentaryReceptionDto(
    Guid Id,
    Guid ContractId,
    Guid CustomerId,
    Guid ReceivedByUserId,
    DateTime ReceivedAtUtc,
    string? InitialObservations,
    DocumentaryReceptionStatus Status,
    Guid? InspectedByUserId,
    DateTime? InspectionCompletedAtUtc,
    bool? SafeToAccept,
    string? InspectionObservations,
    int? DeclaredBoxCount,
    int? DeclaredFolderCount,
    int? CountedBoxCount,
    int? CountedFolderCount,
    string? DiscrepancyReport,
    Guid? CountedByUserId,
    DateTime? CountedAtUtc,
    Guid? ApprovedByUserId,
    DateTime? ApprovedAtUtc,
    Guid? SuspendedByUserId,
    DateTime? SuspendedAtUtc,
    string? SuspensionReason,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record CreateDocumentaryReceptionRequest(
    Guid ContractId,
    string? InitialObservations);

public sealed record RecordReceptionInspectionRequest(
    bool SafeToAccept,
    string? Observations);

public sealed record RecordReceptionCountingRequest(
    int DeclaredBoxCount,
    int DeclaredFolderCount,
    int CountedBoxCount,
    int CountedFolderCount,
    string? DiscrepancyReport);

public sealed record SuspendDocumentaryReceptionRequest(string Reason);
