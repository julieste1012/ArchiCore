using ArchiCore.Domain.Contracts;
using ArchiCore.Domain.Projects;
using ArchiCore.Domain.Warehouses;
using ArchiCore.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArchiCore.Infrastructure.Persistence.Configurations;

internal sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnType("timestamp with time zone");
        builder.Ignore(x => x.DomainEvents);

        builder.Property(x => x.ProjectCode)
            .HasConversion(DomainValueConverters.ProjectCodeConverter)
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(x => x.ContractId).IsRequired();
        builder.HasOne<Contract>().WithMany().HasForeignKey(x => x.ContractId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.CoordinatorId);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.StartDate).HasColumnType("timestamp with time zone");
        builder.Property(x => x.EndDate).HasColumnType("timestamp with time zone");

        builder.HasIndex(x => new { x.ContractId, x.ProjectCode }).IsUnique();

        builder.OwnsMany(x => x.WarehouseAssignments, owned =>
        {
            owned.ToTable("ProjectWarehouseAssignments");
            owned.WithOwner().HasForeignKey("ProjectId");
            owned.Property<Guid>("ProjectId");
            owned.Property(x => x.WarehouseId).HasColumnName("WarehouseId").IsRequired();
            owned.Property(x => x.AssignedFrom).HasColumnType("timestamp with time zone").IsRequired();
            owned.Property(x => x.ReleasedOn).HasColumnType("timestamp with time zone");
            owned.HasKey("ProjectId", nameof(ProjectWarehouseAssignment.WarehouseId), nameof(ProjectWarehouseAssignment.AssignedFrom));
            owned.HasOne<Warehouse>().WithMany().HasForeignKey(x => x.WarehouseId).OnDelete(DeleteBehavior.Restrict);
            owned.HasIndex("ProjectId", nameof(ProjectWarehouseAssignment.WarehouseId))
                .HasFilter("\"ReleasedOn\" IS NULL")
                .IsUnique();
        });

        builder.Navigation(x => x.WarehouseAssignments).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
