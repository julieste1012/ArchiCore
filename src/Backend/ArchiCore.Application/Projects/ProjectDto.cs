using ArchiCore.Domain.Projects;

namespace ArchiCore.Application.Projects;

public sealed record ProjectWarehouseAssignmentDto(
    Guid WarehouseId, DateTime AssignedFrom, DateTime? ReleasedOn, bool IsActive);

public sealed record ProjectDto(
    Guid Id,
    string ProjectCode,
    Guid ContractId,
    Guid? CoordinatorId,
    ProjectStatus Status,
    DateTime? StartDate,
    DateTime? EndDate,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<ProjectWarehouseAssignmentDto> WarehouseAssignments);

public sealed record CreateProjectRequest(string ProjectCode, Guid ContractId);

public sealed record AssignWarehouseRequest(Guid WarehouseId, DateTimeOffset AssignedFrom);

public sealed record ReleaseWarehouseRequest(Guid WarehouseId, DateTimeOffset ReleasedOn);
