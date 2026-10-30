namespace ArchiCore.Integration.Tests.Infrastructure;

// Response shapes as seen on the wire. They are declared here on purpose, instead of reusing the
// Application DTOs, so a renamed or removed JSON property is detected by these tests.

public sealed record DomainEventAuditResponse(Guid Id, string EventType, DateTime OccurredOnUtc, DateTime RecordedAtUtc, string Payload);

public sealed record SecurityAuditResponse(
    Guid Id,
    DateTime OccurredOnUtc,
    string Action,
    string Outcome,
    Guid? ActorUserId,
    string? ActorRole,
    Guid? TargetUserId,
    string? TargetRole);

public sealed record HealthResponse(string Status, string? Database);

public sealed record ProblemResponse(string? Title, int? Status, string? Detail, string? Code);

public sealed record PagedResponse<T>(int Page, int PageSize, int TotalCount, int TotalPages, IReadOnlyList<T> Items);

public sealed record CustomerContractReferenceResponse(Guid ContractId, string ContractNumber);

public sealed record CustomerResponse(
    Guid Id,
    string Nit,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<CustomerContractReferenceResponse> Contracts);

public sealed record WarehouseResponse(
    Guid Id,
    string WarehouseCode,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record ContractProjectReferenceResponse(Guid ProjectId, string ProjectCode);

public sealed record ContractResponse(
    Guid Id,
    string ContractNumber,
    Guid ClientId,
    DateTime StartDate,
    DateTime? EndDate,
    decimal TotalValue,
    string Currency,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<ContractProjectReferenceResponse> Projects);

public sealed record WarehouseAssignmentResponse(
    Guid WarehouseId,
    DateTime AssignedFrom,
    DateTime? ReleasedOn,
    bool IsActive);

public sealed record ProjectResponse(
    Guid Id,
    string ProjectCode,
    Guid ContractId,
    Guid? CoordinatorId,
    string Status,
    DateTime? StartDate,
    DateTime? EndDate,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<WarehouseAssignmentResponse> WarehouseAssignments);


public sealed record DocumentaryReceptionResponse(
    Guid Id,
    Guid ContractId,
    Guid CustomerId,
    Guid ReceivedByUserId,
    DateTime ReceivedAtUtc,
    string? InitialObservations,
    string Status,
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


public sealed record BoxResponse(
    Guid Id,
    string BoxCode,
    string? Barcode,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
