using ArchiCore.Domain.Loans;
using ArchiCore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ArchiCore.Infrastructure.Persistence.Loans;

/// <summary>
/// Claims overdue delivered loans under PostgreSQL row locks and applies the domain transition.
/// SKIP LOCKED lets multiple API replicas share the job without processing the same loan twice.
/// </summary>
public sealed class LoanOverdueProcessor(
    ArchiCoreDbContext dbContext,
    ILogger<LoanOverdueProcessor> logger)
{
    public const int BatchSize = 100;

    public async Task<int> ProcessBatchAsync(
        DateTimeOffset? evaluatedAtUtc = null,
        CancellationToken cancellationToken = default)
    {
        var evaluationTime = (evaluatedAtUtc ?? DateTimeOffset.UtcNow).ToUniversalTime();
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var dueLoans = await dbContext.Loans
            .FromSqlInterpolated($"""
                SELECT *
                FROM "Loans"
                WHERE "Status" = {LoanStatus.Delivered.ToString()}
                  AND "DueAtUtc" < {evaluationTime}
                ORDER BY "DueAtUtc", "Id"
                LIMIT {BatchSize}
                FOR UPDATE SKIP LOCKED
                """)
            .ToListAsync(cancellationToken);

        if (dueLoans.Count == 0)
        {
            await transaction.CommitAsync(cancellationToken);
            return 0;
        }

        foreach (var loan in dueLoans)
        {
            var result = loan.MarkOverdue(evaluationTime);
            if (result.IsFailure)
            {
                throw new InvalidOperationException(
                    $"Loan {loan.Id} was selected as due but rejected the overdue transition: {result.Error.Code}.");
            }
        }

        // ArchiCoreDbContext persists the aggregate updates and generated outbox messages
        // through this same surrounding transaction.
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        logger.LogInformation(
            "Marked {LoanCount} documentary loans as overdue at {EvaluatedAtUtc}.",
            dueLoans.Count,
            evaluationTime);

        return dueLoans.Count;
    }
}
