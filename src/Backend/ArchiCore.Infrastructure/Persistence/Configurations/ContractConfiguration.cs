using ArchiCore.Domain.Contracts;
using ArchiCore.Domain.Customers;
using ArchiCore.Domain.Projects;
using ArchiCore.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArchiCore.Infrastructure.Persistence.Configurations;

internal sealed class ContractConfiguration : IEntityTypeConfiguration<Contract>
{
    public void Configure(EntityTypeBuilder<Contract> builder)
    {
        builder.ToTable("Contracts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnType("timestamp with time zone");
        builder.Ignore(x => x.DomainEvents);

        builder.Property(x => x.ContractNumber)
            .HasConversion(DomainValueConverters.ContractNumberConverter)
            .HasMaxLength(20)
            .IsRequired();
        builder.HasIndex(x => x.ContractNumber).IsUnique();
        builder.Property(x => x.ClientId).IsRequired();
        builder.HasOne<Customer>().WithMany().HasForeignKey(x => x.ClientId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.StartDate).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(x => x.EndDate).HasColumnType("timestamp with time zone");
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();

        builder.OwnsOne(x => x.TotalValue, money =>
        {
            money.Property(x => x.Amount).HasColumnName("TotalValueAmount").HasPrecision(18, 2).IsRequired();
            money.Property(x => x.Currency).HasColumnName("TotalValueCurrency").HasMaxLength(3).IsRequired();
        });
        builder.Navigation(x => x.TotalValue).IsRequired();

        builder.OwnsMany<ContractProjectReference>("_projects", owned =>
        {
            owned.ToTable("ContractProjects");
            owned.WithOwner().HasForeignKey("ContractId");
            owned.Property<Guid>("ContractId");
            owned.Property(x => x.ProjectId).HasColumnName("ProjectId").IsRequired();
            owned.Property(x => x.ProjectCode)
                .HasConversion(DomainValueConverters.ProjectCodeConverter)
                .HasColumnName("ProjectCode")
                .HasMaxLength(20)
                .IsRequired();
            owned.HasKey("ContractId", nameof(ContractProjectReference.ProjectId));
            owned.HasIndex("ContractId", nameof(ContractProjectReference.ProjectCode)).IsUnique();
            owned.HasOne<Project>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Navigation("_projects").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
