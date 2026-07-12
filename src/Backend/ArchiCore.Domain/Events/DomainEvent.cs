namespace ArchiCore.Domain.Events;

/// <summary>
/// Clase base abstracta para todos los eventos de dominio.
/// </summary>
public abstract class DomainEvent : IDomainEvent
{
    /// <inheritdoc />
    public Guid Id { get; } = Guid.NewGuid();

    /// <inheritdoc />
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
