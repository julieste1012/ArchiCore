namespace ArchiCore.Infrastructure.Persistence.Outbox;

public interface IDomainEventOutboxProcessor
{
    /// <summary>Atomically projects pending domain events into the audit trail and marks the rows processed.</summary>
    Task<int> ProcessBatchAsync(CancellationToken cancellationToken = default);
}
