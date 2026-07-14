using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.Contracts.Events;

/// <summary>
/// Evento de dominio generado cuando un <see cref="Contract"/> finaliza.
/// </summary>
public sealed class ContractFinished : DomainEvent
{
    public Guid ContractId { get; }
    public DateTime EndDate { get; }

    public ContractFinished(Guid contractId, DateTime endDate)
    {
        ContractId = contractId;
        EndDate = endDate;
    }
}
