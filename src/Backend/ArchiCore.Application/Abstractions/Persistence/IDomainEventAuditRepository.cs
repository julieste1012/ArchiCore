using ArchiCore.Application.Audit;

namespace ArchiCore.Application.Abstractions.Persistence;

public interface IDomainEventAuditRepository
{
    Task<IReadOnlyList<DomainEventAuditDto>> ListAsync(
        DateTime? fromUtc,
        DateTime? toUtc,
        string? eventType,
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    Task<int> CountAsync(
        DateTime? fromUtc,
        DateTime? toUtc,
        string? eventType,
        CancellationToken cancellationToken = default);
}
