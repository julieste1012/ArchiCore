namespace ArchiCore.Application.Audit;

public sealed record DomainEventAuditDto(
    Guid Id,
    string EventType,
    DateTime OccurredOnUtc,
    DateTime RecordedAtUtc,
    string Payload);
