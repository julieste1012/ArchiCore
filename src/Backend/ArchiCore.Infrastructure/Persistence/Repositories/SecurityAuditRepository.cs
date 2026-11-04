using ArchiCore.Application.Abstractions.Persistence;
using ArchiCore.Application.Audit;
using ArchiCore.Infrastructure.Persistence.Audit;
using Microsoft.EntityFrameworkCore;

namespace ArchiCore.Infrastructure.Persistence.Repositories;

internal sealed class SecurityAuditRepository(ArchiCoreDbContext dbContext) : ISecurityAuditRepository
{
    public async Task<IReadOnlyList<SecurityAuditDto>> ListAsync(
        DateTime? fromUtc,
        DateTime? toUtc,
        string? action,
        string? outcome,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        var query = Filter(dbContext.SecurityAuditEntries.AsNoTracking(), fromUtc, toUtc, action, outcome);
        return await query
            .OrderByDescending(entry => entry.OccurredOnUtc)
            .ThenByDescending(entry => entry.Id)
            .Skip(skip)
            .Take(take)
            .Select(entry => new SecurityAuditDto(
                entry.Id,
                entry.OccurredOnUtc,
                entry.Action,
                entry.Outcome,
                entry.ActorUserId,
                entry.ActorRole,
                entry.TargetUserId,
                entry.TargetRole))
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountAsync(
        DateTime? fromUtc,
        DateTime? toUtc,
        string? action,
        string? outcome,
        CancellationToken cancellationToken = default)
        => Filter(dbContext.SecurityAuditEntries.AsNoTracking(), fromUtc, toUtc, action, outcome)
            .CountAsync(cancellationToken);

    private static IQueryable<SecurityAuditEntry> Filter(
        IQueryable<SecurityAuditEntry> query,
        DateTime? fromUtc,
        DateTime? toUtc,
        string? action,
        string? outcome)
    {
        if (fromUtc.HasValue)
        {
            query = query.Where(entry => entry.OccurredOnUtc >= fromUtc.Value);
        }

        if (toUtc.HasValue)
        {
            query = query.Where(entry => entry.OccurredOnUtc <= toUtc.Value);
        }

        if (!string.IsNullOrWhiteSpace(action))
        {
            query = query.Where(entry => entry.Action == action);
        }

        if (!string.IsNullOrWhiteSpace(outcome))
        {
            query = query.Where(entry => entry.Outcome == outcome);
        }

        return query;
    }
}
