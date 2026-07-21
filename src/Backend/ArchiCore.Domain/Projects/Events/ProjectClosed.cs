using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.Projects.Events;

/// <summary>
/// Evento de dominio generado cuando un <see cref="Project"/> es cerrado.
/// </summary>
public sealed class ProjectClosed : DomainEvent
{
    public Guid ProjectId { get; }

    public ProjectClosed(Guid projectId)
    {
        ProjectId = projectId;
    }
}
