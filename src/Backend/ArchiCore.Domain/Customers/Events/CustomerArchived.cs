using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.Customers.Events;

/// <summary>
/// Evento de dominio generado cuando un <see cref="Customer"/> es archivado.
/// </summary>
public sealed class CustomerArchived : DomainEvent
{
    public Guid CustomerId { get; }

    public CustomerArchived(Guid customerId)
    {
        CustomerId = customerId;
    }
}
