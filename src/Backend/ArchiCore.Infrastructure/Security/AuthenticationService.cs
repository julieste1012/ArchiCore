using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using ArchiCore.Application.Authentication;
using ArchiCore.Domain.Common;
using ArchiCore.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ArchiCore.Infrastructure.Security;

public sealed class AuthenticationService(
    ArchiCoreDbContext dbContext,
    IPasswordHasher<UserAccount> passwordHasher,
    JwtSettings jwtSettings) : IAuthenticationService
{
    private const int MaximumFailedAttempts = 5;

    public async Task<Result<AuthenticationResponse>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(request.Email);
        var user = await dbContext.UserAccounts.SingleOrDefaultAsync(
            candidate => candidate.NormalizedEmail == normalizedEmail,
            cancellationToken);

        // Keep the external response identical for missing users, wrong passwords, inactive
        // accounts and locked accounts to avoid disclosing account state.
        if (user is null || !user.IsActive)
        {
            return InvalidCredentials();
        }

        var now = DateTime.UtcNow;
        if (user.LockoutEndUtc is not null && user.LockoutEndUtc <= now)
        {
            user.ResetFailedLoginState(now);
        }

        if (user.LockoutEndUtc is not null && user.LockoutEndUtc > now)
        {
            return InvalidCredentials();
        }

        var verification = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verification == PasswordVerificationResult.Failed)
        {
            user.RecordFailedLogin(now);
            await dbContext.SaveChangesAsync(cancellationToken);
            return InvalidCredentials();
        }

        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.SetPasswordHash(passwordHasher.HashPassword(user, request.Password));
        }

        user.ResetFailedLoginState(now);
        return Result.Success(await IssueSessionAsync(user, cancellationToken));
    }

    public async Task<Result<AuthenticationResponse>> RefreshAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return InvalidRefreshToken();
        }

        var suppliedHash = HashRefreshToken(request.RefreshToken);
        var existing = await dbContext.RefreshTokens
            .Include(token => token.UserAccount)
            .SingleOrDefaultAsync(token => token.TokenHash == suppliedHash, cancellationToken);

        var now = DateTime.UtcNow;
        if (existing is null
            || existing.RevokedAtUtc is not null
            || existing.ExpiresAtUtc <= now
            || !existing.UserAccount.IsActive)
        {
            return InvalidRefreshToken();
        }

        var newRawToken = GenerateRefreshToken();
        var newHash = HashRefreshToken(newRawToken);
        var newExpires = now.AddDays(jwtSettings.RefreshTokenDays);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        existing.Revoke(now, newHash);
        dbContext.RefreshTokens.Add(
            RefreshTokenRecord.Create(existing.UserAccountId, newHash, now, newExpires));

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken);
            return InvalidRefreshToken();
        }

        return Result.Success(CreateAuthenticationResponse(existing.UserAccount, newRawToken, newExpires, now));
    }

    public async Task RevokeRefreshTokenAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return;
        }

        var hash = HashRefreshToken(request.RefreshToken);
        var token = await dbContext.RefreshTokens.SingleOrDefaultAsync(
            candidate => candidate.TokenHash == hash,
            cancellationToken);

        if (token is null || token.RevokedAtUtc is not null)
        {
            return;
        }

        token.Revoke(DateTime.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Result<UserAccountDto>> GetUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await dbContext.UserAccounts.AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Id == userId && candidate.IsActive, cancellationToken);

        return user is null
            ? Result.Failure<UserAccountDto>(new Error("Authentication.UserNotFound", "The user account was not found."))
            : Result.Success(ToDto(user));
    }

    public async Task<Result<UserAccountDto>> CreateUserAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = request.Email?.Trim() ?? string.Empty;
        var displayName = request.DisplayName?.Trim() ?? string.Empty;
        var password = request.Password ?? string.Empty;
        var role = request.Role?.Trim() ?? string.Empty;

        if (!new EmailAddressAttribute().IsValid(email) || email.Length > 254)
        {
            return Result.Failure<UserAccountDto>(
                new Error("Authentication.InvalidEmail", "A valid email address is required."));
        }

        if (displayName.Length is < 2 or > 120)
        {
            return Result.Failure<UserAccountDto>(
                new Error("Authentication.InvalidDisplayName", "Display name must contain between 2 and 120 characters."));
        }

        if (!AppRoles.All.Contains(role))
        {
            return Result.Failure<UserAccountDto>(
                new Error("Authentication.InvalidRole", "The requested role is not a recognized business role."));
        }

        if (!PasswordPolicy.IsStrong(password))
        {
            return Result.Failure<UserAccountDto>(
                new Error("Authentication.WeakPassword",
                    "Password must be at least 12 characters and include uppercase, lowercase, numeric and non-alphanumeric characters."));
        }

        var normalizedEmail = NormalizeEmail(email);
        if (await dbContext.UserAccounts.AnyAsync(
                candidate => candidate.NormalizedEmail == normalizedEmail,
                cancellationToken))
        {
            return Result.Failure<UserAccountDto>(
                new Error("Authentication.EmailAlreadyExists", "An account with that email address already exists."));
        }

        var account = UserAccount.Create(email, displayName, role);
        account.SetPasswordHash(passwordHasher.HashPassword(account, password));
        dbContext.UserAccounts.Add(account);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // A parallel request can win the unique-email race after the pre-check.
            return Result.Failure<UserAccountDto>(
                new Error("Authentication.EmailAlreadyExists", "An account with that email address already exists."));
        }

        return Result.Success(ToDto(account));
    }

    private async Task<AuthenticationResponse> IssueSessionAsync(
        UserAccount user,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var rawToken = GenerateRefreshToken();
        var expires = now.AddDays(jwtSettings.RefreshTokenDays);

        dbContext.RefreshTokens.Add(
            RefreshTokenRecord.Create(user.Id, HashRefreshToken(rawToken), now, expires));

        await dbContext.SaveChangesAsync(cancellationToken);
        return CreateAuthenticationResponse(user, rawToken, expires, now);
    }

    private AuthenticationResponse CreateAuthenticationResponse(
        UserAccount user,
        string rawRefreshToken,
        DateTime refreshExpiresAtUtc,
        DateTime nowUtc)
    {
        var accessExpires = nowUtc.AddMinutes(jwtSettings.AccessTokenMinutes);
        var signingKey = new SymmetricSecurityKey(Convert.FromBase64String(jwtSettings.SigningKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new System.Security.Claims.Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new System.Security.Claims.Claim(JwtRegisteredClaimNames.Email, user.Email),
            new System.Security.Claims.Claim(JwtRegisteredClaimNames.Name, user.DisplayName),
            new System.Security.Claims.Claim("role", user.Role),
            new System.Security.Claims.Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        };

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = jwtSettings.Issuer,
            Audience = jwtSettings.Audience,
            Subject = new System.Security.Claims.ClaimsIdentity(claims),
            Expires = accessExpires,
            NotBefore = nowUtc,
            SigningCredentials = credentials
        };

        var handler = new JwtSecurityTokenHandler();
        var accessToken = handler.WriteToken(handler.CreateToken(descriptor));

        return new AuthenticationResponse(
            accessToken,
            accessExpires,
            rawRefreshToken,
            refreshExpiresAtUtc,
            ToDto(user));
    }

    private static string NormalizeEmail(string email)
        => (email ?? string.Empty).Trim().ToUpperInvariant();

    private static string GenerateRefreshToken()
        => Microsoft.AspNetCore.WebUtilities.WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(64));

    private static string HashRefreshToken(string rawToken)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));

    private static UserAccountDto ToDto(UserAccount user)
        => new(user.Id, user.Email, user.DisplayName, user.Role, user.IsActive, user.CreatedAtUtc);

    private static Result<AuthenticationResponse> InvalidCredentials()
        => Result.Failure<AuthenticationResponse>(
            new Error("Authentication.InvalidCredentials", "Email or password is invalid."));

    private static Result<AuthenticationResponse> InvalidRefreshToken()
        => Result.Failure<AuthenticationResponse>(
            new Error("Authentication.InvalidRefreshToken", "Refresh token is invalid or expired."));
}

internal static class PasswordPolicy
{
    internal static bool IsStrong(string password)
        => password.Length >= 12
           && password.Any(char.IsUpper)
           && password.Any(char.IsLower)
           && password.Any(char.IsDigit)
           && password.Any(character => !char.IsLetterOrDigit(character));
}
