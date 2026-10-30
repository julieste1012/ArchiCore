using ArchiCore.Domain.Contracts;
using ArchiCore.Domain.ValueObjects;

namespace ArchiCore.Application.Abstractions.Persistence;

public interface IContractRepository
{
    Task<Contract?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Contract?> GetByNumberAsync(ContractNumber contractNumber, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Contract>> ListAsync(Guid? clientId, int skip, int take, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid? clientId, CancellationToken cancellationToken = default);
    Task AddAsync(Contract contract, CancellationToken cancellationToken = default);
}
