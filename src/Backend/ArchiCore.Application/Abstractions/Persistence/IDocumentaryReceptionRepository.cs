using ArchiCore.Domain.DocumentaryReception;

namespace ArchiCore.Application.Abstractions.Persistence;

public interface IDocumentaryReceptionRepository
{
    Task<DocumentaryReception?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DocumentaryReception>> ListAsync(
        Guid? contractId,
        DocumentaryReceptionStatus? status,
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    Task<int> CountAsync(
        Guid? contractId,
        DocumentaryReceptionStatus? status,
        CancellationToken cancellationToken = default);

    Task AddAsync(DocumentaryReception reception, CancellationToken cancellationToken = default);
}
