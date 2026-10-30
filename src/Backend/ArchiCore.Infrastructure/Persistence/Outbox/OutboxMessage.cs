using System.Text.Json;
using ArchiCore.Domain.Events;

namespace ArchiCore.Infrastructure.Persistence.Outbox;

/// <summary>
/// A durable representation of a domain event saved in the same EF Core transaction as aggregate changes.
/// Dispatching and consumer-side audit processing are separate follow-up work.
/// </summary>
public sealed class OutboxMessage
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public Guid Id { get; private set; }

    public string EventType { get; private set; } = null!;

    public DateTime OccurredOnUtc { get; private set; }

    public DateTime EnqueuedAtUtc { get; private set; }

    /// <summary>
    /// JSONB payload serialized using the runtime event type and web JSON naming conventions.
    /// Consumers must use an allow-listed event-type registry, not arbitrary type activation.
    /// </summary>
    public string Payload { get; private set; } = null!;

    public DateTime? ProcessedAtUtc { get; private set; }

    internal void MarkProcessed(DateTime processedAtUtc)
    {
        if (ProcessedAtUtc is not null)
        {
            return;
        }

        ProcessedAtUtc = DateTime.SpecifyKind(processedAtUtc, DateTimeKind.Utc);
    }

    private OutboxMessage()
    {
    }

    internal static OutboxMessage FromDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        var eventType = domainEvent.GetType();
        var eventTypeName = eventType.FullName ?? eventType.Name;

        if (eventTypeName.Length > 512)
        {
            throw new InvalidOperationException("The domain event type name exceeds the outbox column limit.");
        }

        return new OutboxMessage
        {
            Id = domainEvent.Id,
            EventType = eventTypeName,
            OccurredOnUtc = DateTime.SpecifyKind(domainEvent.OccurredOn, DateTimeKind.Utc),
            EnqueuedAtUtc = DateTime.UtcNow,
            Payload = JsonSerializer.Serialize(domainEvent, eventType, SerializerOptions)
        };
    }
}
