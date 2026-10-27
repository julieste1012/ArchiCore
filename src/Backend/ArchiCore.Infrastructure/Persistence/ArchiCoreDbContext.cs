using ArchiCore.Application.Abstractions.Persistence;
using ArchiCore.Domain.Contracts;
using ArchiCore.Domain.Customers;
using ArchiCore.Domain.Projects;
using ArchiCore.Domain.Warehouses;
using Microsoft.EntityFrameworkCore;

namespace ArchiCore.Infrastructure.Persistence;

public sealed class ArchiCoreDbContext(DbContextOptions<ArchiCoreDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ArchiCoreDbContext).Assembly);
    }
}
