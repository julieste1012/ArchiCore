using ArchiCore.Infrastructure.Persistence.Outbox;

namespace ArchiCore.Infrastructure.Persistence.Audit;

/// <summary>
/// Immutable projection of a domain event into the durable technical audit trail.
/// This is not a substitute for user/actor auditing because the current domain events
/// do not consistently carry authenticated actor information.
/// </summary>
public sealed class DomainEventAuditEntry
{
    private DomainEventAuditEntry()
    {
        EventType = string.Empty;
        Payload = string.Empty;
    }

    public Guid Id { get; private set; }
    public string EventType { get; private set; }
    public DateTime OccurredOnUtc { get; private set; }
    public DateTime RecordedAtUtc { get; private set; }
    public string Payload { get; private set; }

    public static DomainEventAuditEntry FromOutboxMessage(OutboxMessage message, DateTime recordedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(message);

        return new DomainEventAuditEntry
        {
            Id = message.Id,
            EventType = message.EventType,
            OccurredOnUtc = DateTime.SpecifyKind(message.OccurredOnUtc, DateTimeKind.Utc),
            RecordedAtUtc = DateTime.SpecifyKind(recordedAtUtc, DateTimeKind.Utc),
            Payload = message.Payload
        };
    }
}
