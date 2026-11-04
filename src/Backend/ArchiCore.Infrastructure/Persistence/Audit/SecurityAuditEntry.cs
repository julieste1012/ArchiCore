namespace ArchiCore.Infrastructure.Persistence.Audit;

/// <summary>
/// Immutable record of an authentication or account-administration action.
/// This stream is intentionally separate from domain-event audit projections.
/// Secrets, passwords, access tokens and refresh tokens must never be written here.
/// </summary>
public sealed class SecurityAuditEntry
{
    private SecurityAuditEntry()
    {
        Action = string.Empty;
        Outcome = string.Empty;
    }

    public Guid Id { get; private set; }
    public DateTime OccurredOnUtc { get; private set; }
    public string Action { get; private set; }
    public string Outcome { get; private set; }
    public Guid? ActorUserId { get; private set; }
    public string? ActorRole { get; private set; }
    public Guid? TargetUserId { get; private set; }
    public string? TargetRole { get; private set; }

    public static SecurityAuditEntry Create(
        string action,
        string outcome,
        Guid? actorUserId = null,
        string? actorRole = null,
        Guid? targetUserId = null,
        string? targetRole = null,
        DateTime? occurredOnUtc = null)
    {
        if (string.IsNullOrWhiteSpace(action) || action.Length > 128)
        {
            throw new ArgumentException("A security audit action between 1 and 128 characters is required.", nameof(action));
        }

        if (string.IsNullOrWhiteSpace(outcome) || outcome.Length > 32)
        {
            throw new ArgumentException("A security audit outcome between 1 and 32 characters is required.", nameof(outcome));
        }

        if (actorRole?.Length > 64 || targetRole?.Length > 64)
        {
            throw new ArgumentException("Audit role identifiers cannot exceed 64 characters.");
        }

        var now = occurredOnUtc ?? DateTime.UtcNow;
        return new SecurityAuditEntry
        {
            Id = Guid.NewGuid(),
            OccurredOnUtc = DateTime.SpecifyKind(now, DateTimeKind.Utc),
            Action = action.Trim(),
            Outcome = outcome.Trim(),
            ActorUserId = actorUserId,
            ActorRole = string.IsNullOrWhiteSpace(actorRole) ? null : actorRole.Trim(),
            TargetUserId = targetUserId,
            TargetRole = string.IsNullOrWhiteSpace(targetRole) ? null : targetRole.Trim()
        };
    }
}
