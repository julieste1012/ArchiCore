using ArchiCore.Domain.Loans;

namespace ArchiCore.Application.Abstractions.Persistence;

public interface ILoanRepository
{
    Task<Loan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Loan>> ListByFolderAsync(Guid folderId, int skip, int take, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Loan>> ListDueAsync(DateTimeOffset evaluatedAtUtc, int skip, int take, CancellationToken cancellationToken = default);
    Task AddAsync(Loan loan, CancellationToken cancellationToken = default);
}
