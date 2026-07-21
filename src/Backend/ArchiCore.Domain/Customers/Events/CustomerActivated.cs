using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.Customers.Events;

/// <summary>
/// Evento de dominio generado cuando un <see cref="Customer"/> es activado.
/// </summary>
public sealed class CustomerActivated : DomainEvent
{
    public Guid CustomerId { get; }

    public CustomerActivated(Guid customerId)
    {
        CustomerId = customerId;
    }
}
