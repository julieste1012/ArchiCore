using ArchiCore.Domain.Common;
using ArchiCore.Domain.Contracts.Events;
using ArchiCore.Domain.ValueObjects;

namespace ArchiCore.Domain.Contracts;

/// <summary>
/// Aggregate Root que representa el acuerdo comercial firmado entre un cliente
/// y MULTISERTEC, y punto de inicio de la operación dentro del ERP.
/// </summary>
public sealed class Contract : Entity, IAggregateRoot
{
    private readonly List<ContractProjectReference> _projects = new();

    /// <summary>
    /// Número identificador del contrato.
    /// </summary>
    public ContractNumber ContractNumber { get; private set; }

    /// <summary>
    /// Referencia al cliente propietario del contrato.
    /// </summary>
    public Guid ClientId { get; private set; }

    /// <summary>
    /// Fecha de inicio del contrato.
    /// </summary>
    public DateTime StartDate { get; private set; }

    /// <summary>
    /// Fecha de finalización del contrato. Se asigna al invocar <see cref="Finish"/>.
    /// </summary>
    public DateTime? EndDate { get; private set; }

    /// <summary>
    /// Valor económico del contrato.
    /// </summary>
    public Money TotalValue { get; private set; }

    /// <summary>
    /// Estado actual del contrato.
    /// </summary>
    public ContractStatus Status { get; private set; }

    /// <summary>
    /// Proyectos asociados al contrato, referenciados por identidad.
    /// </summary>
    public IReadOnlyCollection<ContractProjectReference> Projects => _projects.AsReadOnly();

    private Contract()
    {
        ContractNumber = null!;
        TotalValue = null!;
    }

    private Contract(ContractNumber contractNumber, Guid clientId, DateTime startDate, Money totalValue)
    {
        ContractNumber = contractNumber;
        ClientId = clientId;
        StartDate = startDate;
        TotalValue = totalValue;
        Status = ContractStatus.Draft;
    }

    /// <summary>
    /// Crea un nuevo contrato en estado <see cref="ContractStatus.Draft"/>.
    /// </summary>
    public static Result<Contract> Create(ContractNumber contractNumber, Guid clientId, DateTime startDate, Money totalValue)
    {
        if (contractNumber is null)
        {
            return Result.Failure<Contract>(
                new Error("Contract.ContractNumberRequired", "El número de contrato es obligatorio."));
        }

        if (clientId == Guid.Empty)
        {
            return Result.Failure<Contract>(
                new Error("Contract.ClientRequired", "Un contrato siempre debe pertenecer a un cliente."));
        }

        if (totalValue is null)
        {
            return Result.Failure<Contract>(
                new Error("Contract.TotalValueRequired", "El valor económico del contrato es obligatorio."));
        }

        if (startDate == DateTime.MinValue)
        {
            return Result.Failure<Contract>(
                new Error("Contract.StartDateRequired", "La fecha de inicio del contrato es obligatoria."));
        }

        var contract = new Contract(contractNumber, clientId, startDate, totalValue);

        contract.AddDomainEvent(new ContractCreated(contract.Id, contract.ContractNumber.Value, contract.ClientId));

        return Result.Success(contract);
    }

    /// <summary>
    /// Activa el contrato desde los estados <see cref="ContractStatus.Draft"/> o <see cref="ContractStatus.Suspended"/>.
    /// </summary>
    public Result Activate()
    {
        if (Status != ContractStatus.Draft && Status != ContractStatus.Suspended)
        {
            return Result.Failure(
                new Error("Contract.InvalidActivation", "El contrato solo puede activarse desde los estados Draft o Suspended."));
        }

        Status = ContractStatus.Active;
        MarkAsUpdated();
        AddDomainEvent(new ContractActivated(Id));

        return Result.Success();
    }

    /// <summary>
    /// Suspende el contrato desde el estado <see cref="ContractStatus.Active"/>.
    /// </summary>
    public Result Suspend()
    {
        if (Status != ContractStatus.Active)
        {
            return Result.Failure(
                new Error("Contract.InvalidSuspension", "El contrato solo puede suspenderse desde el estado Active."));
        }

        Status = ContractStatus.Suspended;
        MarkAsUpdated();
        AddDomainEvent(new ContractSuspended(Id));

        return Result.Success();
    }

    /// <summary>
    /// Finaliza el contrato desde el estado <see cref="ContractStatus.Active"/>.
    /// </summary>
    /// <param name="finishDate">Fecha de finalización del contrato.</param>
    public Result Finish(DateTime finishDate)
    {
        if (Status != ContractStatus.Active)
        {
            return Result.Failure(
                new Error("Contract.InvalidFinish", "El contrato solo puede finalizar desde el estado Active."));
        }

        if (finishDate < StartDate)
        {
            return Result.Failure(
                new Error("Contract.FinishBeforeStart", "El contrato no puede finalizar antes de su fecha de inicio."));
        }

        EndDate = finishDate;
        Status = ContractStatus.Finished;
        MarkAsUpdated();
        AddDomainEvent(new ContractFinished(Id, finishDate));

        return Result.Success();
    }

    /// <summary>
    /// Agrega un proyecto al contrato, garantizando la unicidad de su código.
    /// </summary>
    public Result AddProject(Guid projectId, ProjectCode projectCode)
    {
        if (projectId == Guid.Empty)
        {
            return Result.Failure(
                new Error("Contract.ProjectIdRequired", "El identificador del proyecto es obligatorio."));
        }

        if (projectCode is null)
        {
            return Result.Failure(
                new Error("Contract.ProjectCodeRequired", "El código del proyecto es obligatorio."));
        }

        if (ContainsProject(projectCode))
        {
            return Result.Failure(
                new Error("Contract.DuplicateProjectCode", "No pueden existir dos proyectos con el mismo código dentro del contrato."));
        }

        _projects.Add(new ContractProjectReference(projectId, projectCode));
        MarkAsUpdated();
        AddDomainEvent(new ProjectAdded(Id, projectId, projectCode.Value));

        return Result.Success();
    }

    /// <summary>
    /// Elimina un proyecto previamente asociado al contrato.
    /// </summary>
    public Result RemoveProject(Guid projectId)
    {
        var project = _projects.FirstOrDefault(p => p.ProjectId == projectId);

        if (project is null)
        {
            return Result.Failure(
                new Error("Contract.ProjectNotFound", "El proyecto especificado no pertenece a este contrato."));
        }

        _projects.Remove(project);
        MarkAsUpdated();

        return Result.Success();
    }

    private bool ContainsProject(ProjectCode projectCode)
    {
        return _projects.Any(p => p.ProjectCode == projectCode);
    }
}
