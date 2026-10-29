using ArchiCore.Infrastructure.Persistence.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ArchiCore.Infrastructure.Persistence.Outbox;

/// <summary>
/// Uses PostgreSQL row locks to coordinate multiple API instances. Audit projection and
/// processed marking commit in the same transaction, so transient failures leave rows pending.
/// </summary>
public sealed class DomainEventOutboxProcessor(
    ArchiCoreDbContext dbContext,
    ILogger<DomainEventOutboxProcessor> logger) : IDomainEventOutboxProcessor
{
    private const int BatchSize = 100;

    public async Task<int> ProcessBatchAsync(CancellationToken cancellationToken = default)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var pending = await dbContext.OutboxMessages
            .FromSqlRaw("""
                SELECT *
                FROM "OutboxMessages"
                WHERE "ProcessedAtUtc" IS NULL
                ORDER BY "EnqueuedAtUtc", "Id"
                LIMIT 100
                FOR UPDATE SKIP LOCKED
                """)
            .ToListAsync(cancellationToken);

        if (pending.Count == 0)
        {
            await transaction.CommitAsync(cancellationToken);
            return 0;
        }

        var now = DateTime.UtcNow;
        foreach (var message in pending)
        {
            var auditExists = await dbContext.DomainEventAuditEntries
                .AnyAsync(entry => entry.Id == message.Id, cancellationToken);

            if (!auditExists)
            {
                dbContext.DomainEventAuditEntries.Add(
                    DomainEventAuditEntry.FromOutboxMessage(message, now));
            }

            message.MarkProcessed(now);
        }

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            logger.LogInformation("Processed {OutboxCount} domain events into the audit trail.", pending.Count);
            return pending.Count;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
