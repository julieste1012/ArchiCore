using ArchiCore.Application.Abstractions.Persistence;
using ArchiCore.Application.Audit;
using ArchiCore.Infrastructure.Persistence.Audit;
using Microsoft.EntityFrameworkCore;

namespace ArchiCore.Infrastructure.Persistence.Repositories;

internal sealed class DomainEventAuditRepository(ArchiCoreDbContext dbContext) : IDomainEventAuditRepository
{
    public async Task<IReadOnlyList<DomainEventAuditDto>> ListAsync(
        DateTime? fromUtc,
        DateTime? toUtc,
        string? eventType,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        var query = Filter(dbContext.DomainEventAuditEntries.AsNoTracking(), fromUtc, toUtc, eventType);
        return await query
            .OrderByDescending(entry => entry.OccurredOnUtc)
            .ThenByDescending(entry => entry.Id)
            .Skip(skip)
            .Take(take)
            .Select(entry => new DomainEventAuditDto(
                entry.Id,
                entry.EventType,
                entry.OccurredOnUtc,
                entry.RecordedAtUtc,
                entry.Payload))
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountAsync(
        DateTime? fromUtc,
        DateTime? toUtc,
        string? eventType,
        CancellationToken cancellationToken = default)
        => Filter(dbContext.DomainEventAuditEntries.AsNoTracking(), fromUtc, toUtc, eventType)
            .CountAsync(cancellationToken);

    private static IQueryable<DomainEventAuditEntry> Filter(
        IQueryable<DomainEventAuditEntry> query,
        DateTime? fromUtc,
        DateTime? toUtc,
        string? eventType)
    {
        if (fromUtc.HasValue)
        {
            query = query.Where(entry => entry.OccurredOnUtc >= fromUtc.Value);
        }

        if (toUtc.HasValue)
        {
            query = query.Where(entry => entry.OccurredOnUtc <= toUtc.Value);
        }

        if (!string.IsNullOrWhiteSpace(eventType))
        {
            query = query.Where(entry => entry.EventType == eventType);
        }

        return query;
    }
}
