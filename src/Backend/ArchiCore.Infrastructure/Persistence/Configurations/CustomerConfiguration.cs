using ArchiCore.Domain.Contracts;
using ArchiCore.Domain.Customers;
using ArchiCore.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArchiCore.Infrastructure.Persistence.Configurations;

internal sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnType("timestamp with time zone");
        builder.Ignore(x => x.DomainEvents);

        builder.Property(x => x.Nit)
            .HasConversion(DomainValueConverters.Nit)
            .HasColumnName("Nit")
            .HasMaxLength(20)
            .IsRequired();
        builder.HasIndex(x => x.Nit).IsUnique();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();

        builder.OwnsMany<CustomerContractReference>("_contracts", owned =>
        {
            owned.ToTable("CustomerContracts");
            owned.WithOwner().HasForeignKey("CustomerId");
            owned.Property<Guid>("CustomerId");
            owned.Property(x => x.ContractId).HasColumnName("ContractId").IsRequired();
            owned.Property(x => x.ContractNumber)
                .HasConversion(DomainValueConverters.ContractNumber)
                .HasColumnName("ContractNumber")
                .HasMaxLength(20)
                .IsRequired();
            owned.HasKey("CustomerId", nameof(CustomerContractReference.ContractId));
            owned.HasIndex("CustomerId", nameof(CustomerContractReference.ContractNumber)).IsUnique();
            owned.HasOne<Contract>().WithMany().HasForeignKey(x => x.ContractId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Navigation("_contracts").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
