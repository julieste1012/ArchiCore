using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.Projects.Events;

/// <summary>
/// Evento de dominio generado cuando un <see cref="Project"/> se completa.
/// </summary>
public sealed class ProjectCompleted : DomainEvent
{
    public Guid ProjectId { get; }
    public DateTime EndDate { get; }

    public ProjectCompleted(Guid projectId, DateTime endDate)
    {
        ProjectId = projectId;
        EndDate = endDate;
    }
}
