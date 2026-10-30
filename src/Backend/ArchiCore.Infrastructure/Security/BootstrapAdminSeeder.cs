using System.ComponentModel.DataAnnotations;
using ArchiCore.Application.Authentication;
using ArchiCore.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ArchiCore.Infrastructure.Security;

/// <summary>
/// Creates the initial administrator only when explicitly configured by the operator.
/// It never resets or overwrites an existing account.
/// </summary>
public sealed class BootstrapAdminSeeder(
    ArchiCoreDbContext dbContext,
    IPasswordHasher<UserAccount> passwordHasher)
{
    public async Task EnsureAsync(
        string email,
        string password,
        string displayName,
        CancellationToken cancellationToken = default)
    {
        email = email.Trim();
        if (!new EmailAddressAttribute().IsValid(email) || email.Length > 254)
        {
            throw new InvalidOperationException("Bootstrap administrator email is invalid.");
        }

        if (!PasswordPolicy.IsStrong(password))
        {
            throw new InvalidOperationException(
                "Bootstrap administrator password must be at least 12 characters and contain uppercase, lowercase, numeric and non-alphanumeric characters.");
        }

        var normalizedEmail = email.ToUpperInvariant();
        var existing = await dbContext.UserAccounts.SingleOrDefaultAsync(
            account => account.NormalizedEmail == normalizedEmail,
            cancellationToken);

        if (existing is not null)
        {
            if (!existing.IsActive || existing.Role != AppRoles.SystemAdministrator)
            {
                throw new InvalidOperationException(
                    "The configured bootstrap email already belongs to an inactive or non-administrator account. No account was changed.");
            }

            return;
        }

        var name = string.IsNullOrWhiteSpace(displayName) ? "ArchiCore Administrator" : displayName.Trim();
        if (name.Length is < 2 or > 120)
        {
            throw new InvalidOperationException("Bootstrap administrator display name must contain 2 to 120 characters.");
        }

        var admin = UserAccount.Create(email, name, AppRoles.SystemAdministrator);
        admin.SetPasswordHash(passwordHasher.HashPassword(admin, password));
        dbContext.UserAccounts.Add(admin);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
