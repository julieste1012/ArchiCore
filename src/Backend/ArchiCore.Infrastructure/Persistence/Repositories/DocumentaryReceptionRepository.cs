using ArchiCore.Application.Abstractions.Persistence;
using ArchiCore.Domain.DocumentaryReception;
using Microsoft.EntityFrameworkCore;

namespace ArchiCore.Infrastructure.Persistence.Repositories;

internal sealed class DocumentaryReceptionRepository(ArchiCoreDbContext dbContext) : IDocumentaryReceptionRepository
{
    public Task<DocumentaryReception?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => dbContext.DocumentaryReceptions.SingleOrDefaultAsync(reception => reception.Id == id, cancellationToken);

    public async Task<IReadOnlyList<DocumentaryReception>> ListAsync(
        Guid? contractId,
        DocumentaryReceptionStatus? status,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        var query = Filter(dbContext.DocumentaryReceptions.AsNoTracking(), contractId, status);
        return await query
            .OrderByDescending(reception => reception.ReceivedAtUtc)
            .ThenByDescending(reception => reception.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountAsync(
        Guid? contractId,
        DocumentaryReceptionStatus? status,
        CancellationToken cancellationToken = default)
        => Filter(dbContext.DocumentaryReceptions.AsNoTracking(), contractId, status).CountAsync(cancellationToken);

    public async Task AddAsync(DocumentaryReception reception, CancellationToken cancellationToken = default)
        => await dbContext.DocumentaryReceptions.AddAsync(reception, cancellationToken);

    private static IQueryable<DocumentaryReception> Filter(
        IQueryable<DocumentaryReception> query,
        Guid? contractId,
        DocumentaryReceptionStatus? status)
    {
        if (contractId.HasValue)
            query = query.Where(reception => reception.ContractId == contractId.Value);

        if (status.HasValue)
            query = query.Where(reception => reception.Status == status.Value);

        return query;
    }
}
