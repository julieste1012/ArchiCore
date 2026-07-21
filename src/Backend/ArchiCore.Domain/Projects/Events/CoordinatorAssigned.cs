using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.Projects.Events;

/// <summary>
/// Evento de dominio generado cuando se asigna un coordinador a un <see cref="Project"/>.
/// </summary>
public sealed class CoordinatorAssigned : DomainEvent
{
    public Guid ProjectId { get; }
    public Guid CoordinatorId { get; }

    public CoordinatorAssigned(Guid projectId, Guid coordinatorId)
    {
        ProjectId = projectId;
        CoordinatorId = coordinatorId;
    }
}
