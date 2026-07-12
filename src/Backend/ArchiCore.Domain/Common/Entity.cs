using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.Common;

/// <summary>
/// Clase base abstracta para todas las entidades del dominio.
/// Encapsula identidad, marcas de auditoría temporal y el registro de eventos de dominio.
/// </summary>
public abstract class Entity
{
    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>
    /// Identificador único de la entidad.
    /// </summary>
    public Guid Id { get; protected set; }

    /// <summary>
    /// Fecha y hora (UTC) de creación de la entidad.
    /// </summary>
    public DateTime CreatedAt { get; protected set; }

    /// <summary>
    /// Fecha y hora (UTC) de la última actualización de la entidad.
    /// </summary>
    public DateTime? UpdatedAt { get; protected set; }

    /// <summary>
    /// Colección de eventos de dominio pendientes de despacho, generados por esta entidad.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected Entity()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }

    protected Entity(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("El identificador de la entidad no puede ser vacío.", nameof(id));
        }

        Id = id;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Registra un nuevo evento de dominio para ser despachado posteriormente.
    /// </summary>
    /// <param name="domainEvent">Evento de dominio a registrar.</param>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Elimina un evento de dominio previamente registrado.
    /// </summary>
    /// <param name="domainEvent">Evento de dominio a eliminar.</param>
    protected void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Remove(domainEvent);
    }

    /// <summary>
    /// Limpia todos los eventos de dominio registrados en la entidad.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary>
    /// Marca la entidad como actualizada, refrescando la fecha de última modificación.
    /// </summary>
    protected void MarkAsUpdated()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Entity other)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (GetType() != other.GetType())
        {
            return false;
        }

        return Id == other.Id;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(GetType(), Id);
    }

    public static bool operator ==(Entity? left, Entity? right)
    {
        if (left is null && right is null)
        {
            return true;
        }

        if (left is null || right is null)
        {
            return false;
        }

        return left.Equals(right);
    }

    public static bool operator !=(Entity? left, Entity? right)
    {
        return !(left == right);
    }
}
