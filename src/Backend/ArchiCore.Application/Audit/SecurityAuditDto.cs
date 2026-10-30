namespace ArchiCore.Application.Audit;

public sealed record SecurityAuditDto(
    Guid Id,
    DateTime OccurredOnUtc,
    string Action,
    string Outcome,
    Guid? ActorUserId,
    string? ActorRole,
    Guid? TargetUserId,
    string? TargetRole);
