using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.Projects.Events;

/// <summary>
/// Evento de dominio generado cuando un <see cref="Project"/> inicia su ejecución.
/// </summary>
public sealed class ProjectStarted : DomainEvent
{
    public Guid ProjectId { get; }
    public DateTime StartDate { get; }

    public ProjectStarted(Guid projectId, DateTime startDate)
    {
        ProjectId = projectId;
        StartDate = startDate;
    }
}
