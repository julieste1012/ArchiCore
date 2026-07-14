using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.Contracts.Events;

/// <summary>
/// Evento de dominio generado cuando un <see cref="Contract"/> es suspendido.
/// </summary>
public sealed class ContractSuspended : DomainEvent
{
    public Guid ContractId { get; }

    public ContractSuspended(Guid contractId)
    {
        ContractId = contractId;
    }
}
