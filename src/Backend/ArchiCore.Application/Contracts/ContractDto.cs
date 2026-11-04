using ArchiCore.Domain.Contracts;

namespace ArchiCore.Application.Contracts;

public sealed record ContractProjectReferenceDto(Guid ProjectId, string ProjectCode);

public sealed record ContractDto(
    Guid Id,
    string ContractNumber,
    Guid ClientId,
    DateTime StartDate,
    DateTime? EndDate,
    decimal TotalValue,
    string Currency,
    ContractStatus Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<ContractProjectReferenceDto> Projects);

public sealed record CreateContractRequest(
    string ContractNumber,
    Guid ClientId,
    DateTimeOffset StartDate,
    decimal TotalValue,
    string Currency);

public sealed record FinishContractRequest(DateTimeOffset FinishDate);
