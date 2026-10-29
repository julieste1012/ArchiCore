namespace ArchiCore.Infrastructure.Security;

/// <summary>
/// Technical authentication account. This is deliberately separate from the future
/// business-domain User aggregate, whose personnel/domain boundaries are still under review.
/// </summary>
public sealed class UserAccount
{
    private UserAccount()
    {
        Email = string.Empty;
        NormalizedEmail = string.Empty;
        DisplayName = string.Empty;
        PasswordHash = string.Empty;
        Role = string.Empty;
    }

    public Guid Id { get; private set; }
    public string Email { get; private set; }
    public string NormalizedEmail { get; private set; }
    public string DisplayName { get; private set; }
    public string PasswordHash { get; private set; }
    public string Role { get; private set; }
    public bool IsActive { get; private set; }
    public int TokenVersion { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }
    public int FailedLoginAttempts { get; private set; }
    public DateTime? LockoutEndUtc { get; private set; }

    public ICollection<RefreshTokenRecord> RefreshTokens { get; private set; } = new List<RefreshTokenRecord>();

    public static UserAccount Create(string email, string displayName, string role)
    {
        var normalizedEmail = email.Trim().ToUpperInvariant();
        return new UserAccount
        {
            Id = Guid.NewGuid(),
            Email = email.Trim(),
            NormalizedEmail = normalizedEmail,
            DisplayName = displayName.Trim(),
            PasswordHash = string.Empty,
            Role = role,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow,
            FailedLoginAttempts = 0
        };
    }

    internal void SetActive(bool isActive, DateTime nowUtc)
    {
        if (IsActive == isActive)
        {
            return;
        }

        IsActive = isActive;
        TokenVersion++;
        UpdatedAtUtc = nowUtc;

        if (isActive)
        {
            ResetFailedLoginState(nowUtc);
        }
    }

    internal void SetPasswordHash(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
        {
            throw new ArgumentException("Password hash cannot be empty.", nameof(hash));
        }

        PasswordHash = hash;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    internal void ChangePasswordHash(string hash, DateTime nowUtc)
    {
        if (string.IsNullOrWhiteSpace(hash))
        {
            throw new ArgumentException("Password hash cannot be empty.", nameof(hash));
        }

        PasswordHash = hash;
        TokenVersion++;
        ResetFailedLoginState(nowUtc);
    }

    internal void RecordFailedLogin(DateTime nowUtc)
    {
        FailedLoginAttempts++;
        UpdatedAtUtc = nowUtc;

        if (FailedLoginAttempts >= 5)
        {
            LockoutEndUtc = nowUtc.AddMinutes(15);
        }
    }

    internal void ResetFailedLoginState(DateTime nowUtc)
    {
        FailedLoginAttempts = 0;
        LockoutEndUtc = null;
        UpdatedAtUtc = nowUtc;
    }
}
