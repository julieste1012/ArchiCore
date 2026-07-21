using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.Projects.Events;

/// <summary>
/// Evento de dominio generado cuando un <see cref="Project"/> es archivado.
/// </summary>
public sealed class ProjectArchived : DomainEvent
{
    public Guid ProjectId { get; }

    public ProjectArchived(Guid projectId)
    {
        ProjectId = projectId;
    }
}
