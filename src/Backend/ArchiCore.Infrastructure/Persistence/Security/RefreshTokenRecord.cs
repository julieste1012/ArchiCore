namespace ArchiCore.Infrastructure.Security;

/// <summary>
/// Stores only a one-way hash of a refresh token. Revoke/replace metadata supports rotation
/// and prevents the same refresh token from being redeemed more than once.
/// </summary>
public sealed class RefreshTokenRecord
{
    private RefreshTokenRecord()
    {
        TokenHash = string.Empty;
        ReplacedByTokenHash = null;
    }

    public Guid Id { get; private set; }
    public Guid UserAccountId { get; private set; }
    public UserAccount UserAccount { get; private set; } = null!;
    public string TokenHash { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime? RevokedAtUtc { get; private set; }
    public string? ReplacedByTokenHash { get; private set; }

    public static RefreshTokenRecord Create(
        Guid userAccountId,
        string tokenHash,
        DateTime createdAtUtc,
        DateTime expiresAtUtc)
        => new()
        {
            Id = Guid.NewGuid(),
            UserAccountId = userAccountId,
            TokenHash = tokenHash,
            CreatedAtUtc = createdAtUtc,
            ExpiresAtUtc = expiresAtUtc
        };

    internal void Revoke(DateTime nowUtc, string? replacementHash = null)
    {
        if (RevokedAtUtc is not null)
        {
            return;
        }

        RevokedAtUtc = nowUtc;
        ReplacedByTokenHash = replacementHash;
    }
}
