using ArchiCore.Application.Abstractions.Persistence;
using ArchiCore.Domain.Common;
using ArchiCore.Domain.Contracts;
using ArchiCore.Domain.Customers;
using ArchiCore.Domain.Events;
using ArchiCore.Domain.Projects;
using ArchiCore.Domain.Warehouses;
using ArchiCore.Infrastructure.Persistence.Outbox;
using ArchiCore.Infrastructure.Persistence.Audit;
using ArchiCore.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace ArchiCore.Infrastructure.Persistence;

public sealed class ArchiCoreDbContext(DbContextOptions<ArchiCoreDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<DomainEventAuditEntry> DomainEventAuditEntries => Set<DomainEventAuditEntry>();
    public DbSet<UserAccount> UserAccounts => Set<UserAccount>();
    public DbSet<RefreshTokenRecord> RefreshTokens => Set<RefreshTokenRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ArchiCoreDbContext).Assembly);
    }

    public override int SaveChanges() => SaveChanges(acceptAllChangesOnSuccess: true);

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        var pendingEvents = CaptureAndEnqueueDomainEvents();
        var affectedRows = base.SaveChanges(acceptAllChangesOnSuccess);
        ClearPersistedDomainEvents(pendingEvents);
        return affectedRows;
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => SaveChangesAsync(acceptAllChangesOnSuccess: true, cancellationToken);

    public override async Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        var pendingEvents = CaptureAndEnqueueDomainEvents();
        var affectedRows = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        ClearPersistedDomainEvents(pendingEvents);
        return affectedRows;
    }

    /// <summary>
    /// Adds pending aggregate events to the outbox before EF Core saves changes.
    /// EF Core's relational SaveChanges transaction persists aggregate changes and outbox rows atomically.
    /// Events remain on the aggregate if saving fails, allowing a retry without creating duplicate tracked rows.
    /// </summary>
    private IReadOnlyList<(Entity Aggregate, IDomainEvent Event)> CaptureAndEnqueueDomainEvents()
    {
        var aggregates = ChangeTracker.Entries<Entity>()
            .Select(entry => entry.Entity)
            .Distinct()
            .ToArray();

        var pendingEvents = aggregates
            .SelectMany(aggregate => aggregate.DomainEvents.Select(domainEvent => (Aggregate: aggregate, Event: domainEvent)))
            .ToArray();

        foreach (var item in pendingEvents)
        {
            var alreadyTracked = ChangeTracker.Entries<OutboxMessage>()
                .Any(entry => entry.Entity.Id == item.Event.Id);

            if (!alreadyTracked)
            {
                OutboxMessages.Add(OutboxMessage.FromDomainEvent(item.Event));
            }
        }

        return pendingEvents;
    }

    private static void ClearPersistedDomainEvents(
        IReadOnlyList<(Entity Aggregate, IDomainEvent Event)> pendingEvents)
    {
        // Only clear after SaveChanges succeeds. If saving throws, the aggregate keeps its events.
        foreach (var aggregate in pendingEvents.Select(item => item.Aggregate).Distinct())
        {
            aggregate.ClearDomainEvents();
        }
    }
}
