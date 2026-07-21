using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.Projects.Events;

/// <summary>
/// Evento de dominio generado cuando un <see cref="Project"/> es reanudado.
/// </summary>
public sealed class ProjectResumed : DomainEvent
{
    public Guid ProjectId { get; }

    public ProjectResumed(Guid projectId)
    {
        ProjectId = projectId;
    }
}
