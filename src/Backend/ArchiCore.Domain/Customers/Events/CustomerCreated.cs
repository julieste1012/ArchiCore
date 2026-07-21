using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.Customers.Events;

/// <summary>
/// Evento de dominio generado cuando un <see cref="Customer"/> es creado.
/// </summary>
public sealed class CustomerCreated : DomainEvent
{
    public Guid CustomerId { get; }
    public string Nit { get; }

    public CustomerCreated(Guid customerId, string nit)
    {
        CustomerId = customerId;
        Nit = nit;
    }
}
