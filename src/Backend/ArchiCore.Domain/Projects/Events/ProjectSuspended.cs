using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.Projects.Events;

/// <summary>
/// Evento de dominio generado cuando un <see cref="Project"/> es suspendido.
/// </summary>
public sealed class ProjectSuspended : DomainEvent
{
    public Guid ProjectId { get; }

    public ProjectSuspended(Guid projectId)
    {
        ProjectId = projectId;
    }
}
