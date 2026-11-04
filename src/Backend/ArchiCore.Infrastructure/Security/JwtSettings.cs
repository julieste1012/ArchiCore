namespace ArchiCore.Infrastructure.Security;

/// <summary>
/// JWT settings are supplied from environment variables/user-secrets, not committed secrets.
/// SigningKey is base64-encoded random key material of at least 32 bytes.
/// </summary>
public sealed class JwtSettings
{
    public string Issuer { get; init; } = "ArchiCore";
    public string Audience { get; init; } = "ArchiCore.Client";
    public string SigningKey { get; init; } = string.Empty;
    public int AccessTokenMinutes { get; init; } = 15;
    public int RefreshTokenDays { get; init; } = 14;
}
