using ArchiCore.Application.Abstractions.Persistence;
using ArchiCore.Application.Common;
using ArchiCore.Domain.Common;
using ArchiCore.Domain.Projects;
using ArchiCore.Domain.ValueObjects;

namespace ArchiCore.Application.Projects;

public sealed class ProjectService(
    IProjectRepository projects,
    IContractRepository contracts,
    IWarehouseRepository warehouses,
    IUnitOfWork unitOfWork)
{
    public async Task<PagedResult<ProjectDto>> ListAsync(
        Guid? contractId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        (page, pageSize) = NormalizePage(page, pageSize);
        var skip = (page - 1) * pageSize;
        var items = await projects.ListAsync(contractId, skip, pageSize, cancellationToken);
        var count = await projects.CountAsync(contractId, cancellationToken);
        return new PagedResult<ProjectDto>(page, pageSize, count, items.Select(ToDto).ToArray());
    }

    public async Task<Result<ProjectDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var project = await projects.GetByIdAsync(id, cancellationToken);
        return project is null
            ? Result.Failure<ProjectDto>(new Error("Project.NotFound", "No se encontró el proyecto solicitado."))
            : Result.Success(ToDto(project));
    }

    public async Task<Result<ProjectDto>> CreateAsync(CreateProjectRequest request, CancellationToken cancellationToken = default)
    {
        var codeResult = ProjectCode.Create(request.ProjectCode);
        if (codeResult.IsFailure) return Result.Failure<ProjectDto>(codeResult.Error);

        var contract = await contracts.GetByIdAsync(request.ContractId, cancellationToken);
        if (contract is null)
            return Result.Failure<ProjectDto>(new Error("Project.ContractNotFound", "El contrato indicado no existe."));

        if (await projects.GetByContractAndCodeAsync(request.ContractId, codeResult.Value, cancellationToken) is not null)
            return Result.Failure<ProjectDto>(
                new Error("Project.DuplicateProjectCode", "Ya existe un proyecto con ese código dentro del contrato."));

        var createResult = Project.Create(codeResult.Value, request.ContractId);
        if (createResult.IsFailure) return Result.Failure<ProjectDto>(createResult.Error);

        var association = contract.AddProject(createResult.Value.Id, codeResult.Value);
        if (association.IsFailure) return Result.Failure<ProjectDto>(association.Error);

        await projects.AddAsync(createResult.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(ToDto(createResult.Value));
    }

    public Task<Result<ProjectDto>> PlanAsync(Guid id, CancellationToken cancellationToken = default)
        => ExecuteTransitionAsync(id, project => project.Plan(), cancellationToken);
    public Task<Result<ProjectDto>> StartAsync(Guid id, CancellationToken cancellationToken = default)
        => ExecuteTransitionAsync(id, project => project.Start(), cancellationToken);
    public Task<Result<ProjectDto>> SuspendAsync(Guid id, CancellationToken cancellationToken = default)
        => ExecuteTransitionAsync(id, project => project.Suspend(), cancellationToken);
    public Task<Result<ProjectDto>> ResumeAsync(Guid id, CancellationToken cancellationToken = default)
        => ExecuteTransitionAsync(id, project => project.Resume(), cancellationToken);
    public Task<Result<ProjectDto>> CompleteAsync(Guid id, CancellationToken cancellationToken = default)
        => ExecuteTransitionAsync(id, project => project.Complete(), cancellationToken);
    public Task<Result<ProjectDto>> CloseAsync(Guid id, CancellationToken cancellationToken = default)
        => ExecuteTransitionAsync(id, project => project.Close(), cancellationToken);
    public Task<Result<ProjectDto>> ArchiveAsync(Guid id, CancellationToken cancellationToken = default)
        => ExecuteTransitionAsync(id, project => project.Archive(), cancellationToken);

    public async Task<Result<ProjectDto>> AssignWarehouseAsync(
        Guid id, AssignWarehouseRequest request, CancellationToken cancellationToken = default)
    {
        var project = await projects.GetByIdAsync(id, cancellationToken);
        if (project is null)
            return Result.Failure<ProjectDto>(new Error("Project.NotFound", "No se encontró el proyecto solicitado."));

        if (await warehouses.GetByIdAsync(request.WarehouseId, cancellationToken) is null)
            return Result.Failure<ProjectDto>(new Error("Project.WarehouseNotFound", "La bodega indicada no existe."));

        var result = project.AssignWarehouse(request.WarehouseId, request.AssignedFrom.UtcDateTime);
        if (result.IsFailure) return Result.Failure<ProjectDto>(result.Error);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(ToDto(project));
    }

    public async Task<Result<ProjectDto>> ReleaseWarehouseAsync(
        Guid id, ReleaseWarehouseRequest request, CancellationToken cancellationToken = default)
    {
        var project = await projects.GetByIdAsync(id, cancellationToken);
        if (project is null)
            return Result.Failure<ProjectDto>(new Error("Project.NotFound", "No se encontró el proyecto solicitado."));

        var result = project.ReleaseWarehouse(request.WarehouseId, request.ReleasedOn.UtcDateTime);
        if (result.IsFailure) return Result.Failure<ProjectDto>(result.Error);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(ToDto(project));
    }

    private async Task<Result<ProjectDto>> ExecuteTransitionAsync(
        Guid id, Func<Project, Result> transition, CancellationToken cancellationToken)
    {
        var project = await projects.GetByIdAsync(id, cancellationToken);
        if (project is null)
            return Result.Failure<ProjectDto>(new Error("Project.NotFound", "No se encontró el proyecto solicitado."));

        var result = transition(project);
        if (result.IsFailure) return Result.Failure<ProjectDto>(result.Error);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(ToDto(project));
    }

    private static ProjectDto ToDto(Project project)
        => new(
            project.Id,
            project.ProjectCode.Value,
            project.ContractId,
            project.CoordinatorId,
            project.Status,
            project.StartDate,
            project.EndDate,
            project.CreatedAt,
            project.UpdatedAt,
            project.WarehouseAssignments.Select(w =>
                new ProjectWarehouseAssignmentDto(w.WarehouseId, w.AssignedFrom, w.ReleasedOn, w.IsActive)).ToArray());

    private static (int Page, int PageSize) NormalizePage(int page, int pageSize)
        => (Math.Max(1, page), Math.Clamp(pageSize, 1, 100));
}
