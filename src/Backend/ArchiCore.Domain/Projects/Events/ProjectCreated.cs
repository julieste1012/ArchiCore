using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.Projects.Events;

/// <summary>
/// Evento de dominio generado cuando un <see cref="Project"/> es creado.
/// </summary>
public sealed class ProjectCreated : DomainEvent
{
    public Guid ProjectId { get; }
    public string ProjectCode { get; }
    public Guid ContractId { get; }

    public ProjectCreated(Guid projectId, string projectCode, Guid contractId)
    {
        ProjectId = projectId;
        ProjectCode = projectCode;
        ContractId = contractId;
    }
}
