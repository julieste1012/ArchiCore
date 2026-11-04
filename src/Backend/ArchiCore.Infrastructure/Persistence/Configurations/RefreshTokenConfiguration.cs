using ArchiCore.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArchiCore.Infrastructure.Persistence.Configurations;

internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshTokenRecord>
{
    public void Configure(EntityTypeBuilder<RefreshTokenRecord> builder)
    {
        builder.ToTable("RefreshTokens");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.TokenHash).HasMaxLength(64).IsRequired();
        builder.HasIndex(x => x.TokenHash).HasDatabaseName("IX_RefreshTokens_TokenHash").IsUnique();

        builder.Property(x => x.CreatedAtUtc).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(x => x.ExpiresAtUtc).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(x => x.RevokedAtUtc)
            .HasColumnType("timestamp with time zone")
            .IsConcurrencyToken();
        builder.Property(x => x.ReplacedByTokenHash).HasMaxLength(64);

        builder.HasIndex(x => x.UserAccountId).HasDatabaseName("IX_RefreshTokens_UserAccountId");
    }
}
