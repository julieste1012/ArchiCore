using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.Contracts.Events;

/// <summary>
/// Evento de dominio generado cuando un <see cref="Contract"/> es creado.
/// </summary>
public sealed class ContractCreated : DomainEvent
{
    public Guid ContractId { get; }
    public string ContractNumber { get; }
    public Guid ClientId { get; }

    public ContractCreated(Guid contractId, string contractNumber, Guid clientId)
    {
        ContractId = contractId;
        ContractNumber = contractNumber;
        ClientId = clientId;
    }
}
