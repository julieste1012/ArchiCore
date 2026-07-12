namespace ArchiCore.Domain.Events;

/// <summary>
/// Contrato base que deben implementar todos los eventos de dominio.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Identificador único del evento.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Fecha y hora (UTC) en la que ocurrió el evento.
    /// </summary>
    DateTime OccurredOn { get; }
}
