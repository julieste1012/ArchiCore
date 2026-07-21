using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.Customers.Events;

/// <summary>
/// Evento de dominio generado cuando un <see cref="Customer"/> es desactivado.
/// </summary>
public sealed class CustomerDeactivated : DomainEvent
{
    public Guid CustomerId { get; }

    public CustomerDeactivated(Guid customerId)
    {
        CustomerId = customerId;
    }
}
