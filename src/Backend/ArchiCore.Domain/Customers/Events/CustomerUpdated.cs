using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.Customers.Events;

/// <summary>
/// Evento de dominio generado cuando un <see cref="Customer"/> es actualizado.
/// </summary>
public sealed class CustomerUpdated : DomainEvent
{
    public Guid CustomerId { get; }

    public CustomerUpdated(Guid customerId)
    {
        CustomerId = customerId;
    }
}
