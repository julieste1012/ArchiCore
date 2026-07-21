using ArchiCore.Domain.Common;
using ArchiCore.Domain.Projects.Events;
using ArchiCore.Domain.ValueObjects;

namespace ArchiCore.Domain.Projects;

/// <summary>
/// Aggregate Root que representa la ejecución operativa de un contrato.
/// </summary>
public sealed class Project : Entity, IAggregateRoot
{
    private readonly List<ProjectWarehouseAssignment> _warehouseAssignments = new();

    /// <summary>
    /// Código de negocio del proyecto.
    /// </summary>
    public ProjectCode ProjectCode { get; private set; }

    /// <summary>
    /// Referencia al contrato al que pertenece el proyecto.
    /// </summary>
    public Guid ContractId { get; private set; }

    /// <summary>
    /// Referencia al usuario actualmente asignado como coordinador del proyecto.
    /// </summary>
    public Guid? CoordinatorId { get; private set; }

    /// <summary>
    /// Estado actual del proyecto.
    /// </summary>
    public ProjectStatus Status { get; private set; }

    /// <summary>
    /// Fecha en la que el proyecto pasó a ejecución activa.
    /// </summary>
    public DateTime? StartDate { get; private set; }

    /// <summary>
    /// Fecha en la que el proyecto se completó operativamente.
    /// </summary>
    public DateTime? EndDate { get; private set; }

    /// <summary>
    /// Bodegas asignadas al proyecto, referenciadas por identidad y vigencia temporal.
    /// </summary>
    public IReadOnlyCollection<ProjectWarehouseAssignment> WarehouseAssignments => _warehouseAssignments.AsReadOnly();

    private Project(ProjectCode projectCode, Guid contractId)
    {
        ProjectCode = projectCode;
        ContractId = contractId;
        Status = ProjectStatus.Draft;
    }

    /// <summary>
    /// Crea un nuevo proyecto en estado <see cref="ProjectStatus.Draft"/>.
    /// </summary>
    public static Result<Project> Create(ProjectCode projectCode, Guid contractId)
    {
        if (projectCode is null)
        {
            return Result.Failure<Project>(
                new Error("Project.ProjectCodeRequired", "El código del proyecto es obligatorio."));
        }

        if (contractId == Guid.Empty)
        {
            return Result.Failure<Project>(
                new Error("Project.ContractIdRequired", "El proyecto debe pertenecer a un contrato."));
        }

        var project = new Project(projectCode, contractId);

        project.AddDomainEvent(new ProjectCreated(project.Id, project.ProjectCode.Value, project.ContractId));

        return Result.Success(project);
    }

    /// <summary>
    /// Planifica el proyecto desde el estado <see cref="ProjectStatus.Draft"/>.
    /// </summary>
    public Result Plan()
    {
        var validation = EnsureStatusIs(
            ProjectStatus.Draft,
            "Project.InvalidPlanning",
            "El proyecto solo puede planificarse desde el estado Draft.");

        if (validation.IsFailure)
        {
            return validation;
        }

        Status = ProjectStatus.Planned;
        MarkAsUpdated();

        return Result.Success();
    }

    /// <summary>
    /// Inicia el proyecto desde el estado <see cref="ProjectStatus.Planned"/>.
    /// </summary>
    public Result Start()
    {
        var validation = EnsureStatusIs(
            ProjectStatus.Planned,
            "Project.InvalidStart",
            "El proyecto solo puede iniciarse desde el estado Planned.");

        if (validation.IsFailure)
        {
            return validation;
        }

        Status = ProjectStatus.Active;
        StartDate = DateTime.UtcNow;
        MarkAsUpdated();
        AddDomainEvent(new ProjectStarted(Id, StartDate.Value));

        return Result.Success();
    }

    /// <summary>
    /// Suspende el proyecto desde el estado <see cref="ProjectStatus.Active"/>.
    /// </summary>
    public Result Suspend()
    {
        var validation = EnsureStatusIs(
            ProjectStatus.Active,
            "Project.InvalidSuspension",
            "El proyecto solo puede suspenderse desde el estado Active.");

        if (validation.IsFailure)
        {
            return validation;
        }

        Status = ProjectStatus.Suspended;
        MarkAsUpdated();
        AddDomainEvent(new ProjectSuspended(Id));

        return Result.Success();
    }

    /// <summary>
    /// Reanuda el proyecto desde el estado <see cref="ProjectStatus.Suspended"/>.
    /// </summary>
    public Result Resume()
    {
        var validation = EnsureStatusIs(
            ProjectStatus.Suspended,
            "Project.InvalidResume",
            "El proyecto solo puede reanudarse desde el estado Suspended.");

        if (validation.IsFailure)
        {
            return validation;
        }

        Status = ProjectStatus.Active;
        MarkAsUpdated();
        AddDomainEvent(new ProjectResumed(Id));

        return Result.Success();
    }

    /// <summary>
    /// Completa el proyecto desde el estado <see cref="ProjectStatus.Active"/>.
    /// </summary>
    public Result Complete()
    {
        var validation = EnsureStatusIs(
            ProjectStatus.Active,
            "Project.InvalidCompletion",
            "El proyecto solo puede completarse desde el estado Active.");

        if (validation.IsFailure)
        {
            return validation;
        }

        Status = ProjectStatus.Completed;
        EndDate = DateTime.UtcNow;
        MarkAsUpdated();
        AddDomainEvent(new ProjectCompleted(Id, EndDate.Value));

        return Result.Success();
    }

    /// <summary>
    /// Cierra el proyecto desde el estado <see cref="ProjectStatus.Completed"/>.
    /// </summary>
    public Result Close()
    {
        var validation = EnsureStatusIs(
            ProjectStatus.Completed,
            "Project.InvalidClose",
            "El proyecto solo puede cerrarse desde el estado Completed.");

        if (validation.IsFailure)
        {
            return validation;
        }

        Status = ProjectStatus.Closed;
        MarkAsUpdated();
        AddDomainEvent(new ProjectClosed(Id));

        return Result.Success();
    }

    /// <summary>
    /// Archiva el proyecto desde el estado <see cref="ProjectStatus.Closed"/>.
    /// </summary>
    public Result Archive()
    {
        var validation = EnsureStatusIs(
            ProjectStatus.Closed,
            "Project.InvalidArchival",
            "El proyecto solo puede archivarse desde el estado Closed.");

        if (validation.IsFailure)
        {
            return validation;
        }

        Status = ProjectStatus.Archived;
        MarkAsUpdated();
        AddDomainEvent(new ProjectArchived(Id));

        return Result.Success();
    }

    /// <summary>
    /// Asigna un coordinador al proyecto, reemplazando al anterior si existía.
    /// </summary>
    public Result AssignCoordinator(Guid coordinatorId)
    {
        if (coordinatorId == Guid.Empty)
        {
            return Result.Failure(
                new Error("Project.CoordinatorIdRequired", "El identificador del coordinador es obligatorio."));
        }

        CoordinatorId = coordinatorId;
        MarkAsUpdated();
        AddDomainEvent(new CoordinatorAssigned(Id, coordinatorId));

        return Result.Success();
    }

    /// <summary>
    /// Asigna una bodega al proyecto, impidiendo dos asignaciones activas de la misma bodega.
    /// </summary>
    public Result AssignWarehouse(Guid warehouseId, DateTime assignedFrom)
    {
        if (warehouseId == Guid.Empty)
        {
            return Result.Failure(
                new Error("Project.WarehouseIdRequired", "El identificador de la bodega es obligatorio."));
        }

        if (ContainsActiveWarehouseAssignment(warehouseId))
        {
            return Result.Failure(
                new Error("Project.DuplicateWarehouseAssignment", "No puede existir más de una asignación activa de la misma bodega."));
        }

        _warehouseAssignments.Add(new ProjectWarehouseAssignment(warehouseId, assignedFrom));
        MarkAsUpdated();
        AddDomainEvent(new WarehouseAssigned(Id, warehouseId, assignedFrom));

        return Result.Success();
    }

    /// <summary>
    /// Libera la asignación activa de una bodega en el proyecto.
    /// </summary>
    public Result ReleaseWarehouse(Guid warehouseId, DateTime releasedOn)
    {
        var assignment = _warehouseAssignments.FirstOrDefault(w => w.WarehouseId == warehouseId && w.IsActive);

        if (assignment is null)
        {
            return Result.Failure(
                new Error("Project.WarehouseAssignmentNotFound", "No existe una asignación activa de la bodega especificada."));
        }

        if (releasedOn < assignment.AssignedFrom)
        {
            return Result.Failure(
                new Error("Project.InvalidReleaseDate", "La fecha de liberación no puede ser anterior a la fecha de asignación."));
        }

        assignment.Release(releasedOn);
        MarkAsUpdated();
        AddDomainEvent(new WarehouseReleased(Id, warehouseId, releasedOn));

        return Result.Success();
    }

    private Result EnsureStatusIs(ProjectStatus expected, string errorCode, string errorMessage)
    {
        if (Status != expected)
        {
            return Result.Failure(new Error(errorCode, errorMessage));
        }

        return Result.Success();
    }

    private bool ContainsActiveWarehouseAssignment(Guid warehouseId)
    {
        return _warehouseAssignments.Any(w => w.WarehouseId == warehouseId && w.IsActive);
    }
}
