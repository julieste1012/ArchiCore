using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.Contracts.Events;

/// <summary>
/// Evento de dominio generado cuando un proyecto es agregado a un <see cref="Contract"/>.
/// </summary>
public sealed class ProjectAdded : DomainEvent
{
    public Guid ContractId { get; }
    public Guid ProjectId { get; }
    public string ProjectCode { get; }

    public ProjectAdded(Guid contractId, Guid projectId, string projectCode)
    {
        ContractId = contractId;
        ProjectId = projectId;
        ProjectCode = projectCode;
    }
}
