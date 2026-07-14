using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.Contracts.Events;

/// <summary>
/// Evento de dominio generado cuando un <see cref="Contract"/> es activado.
/// </summary>
public sealed class ContractActivated : DomainEvent
{
    public Guid ContractId { get; }

    public ContractActivated(Guid contractId)
    {
        ContractId = contractId;
    }
}
