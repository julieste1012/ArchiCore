using ArchiCore.Application.Abstractions.Persistence;
using ArchiCore.Domain.Contracts;
using ArchiCore.Domain.Customers;
using ArchiCore.Domain.Projects;
using ArchiCore.Domain.ValueObjects;
using ArchiCore.Domain.Warehouses;
using Microsoft.EntityFrameworkCore;

namespace ArchiCore.Infrastructure.Persistence.Repositories;

internal sealed class CustomerRepository(ArchiCoreDbContext dbContext) : ICustomerRepository
{
    public Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => dbContext.Customers.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<Customer?> GetByNitAsync(Nit nit, CancellationToken cancellationToken = default)
        => dbContext.Customers.SingleOrDefaultAsync(x => x.Nit == nit, cancellationToken);

    public async Task<IReadOnlyList<Customer>> ListAsync(int skip, int take, CancellationToken cancellationToken = default)
        => await dbContext.Customers.AsNoTracking().OrderBy(x => x.CreatedAt).ThenBy(x => x.Id)
            .Skip(skip).Take(take).ToListAsync(cancellationToken);

    public Task<int> CountAsync(CancellationToken cancellationToken = default)
        => dbContext.Customers.CountAsync(cancellationToken);

    public async Task AddAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        await dbContext.Customers.AddAsync(customer, cancellationToken);
    }
}

internal sealed class ContractRepository(ArchiCoreDbContext dbContext) : IContractRepository
{
    public Task<Contract?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => dbContext.Contracts.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<Contract?> GetByNumberAsync(ContractNumber contractNumber, CancellationToken cancellationToken = default)
        => dbContext.Contracts.SingleOrDefaultAsync(x => x.ContractNumber == contractNumber, cancellationToken);

    public async Task<IReadOnlyList<Contract>> ListAsync(Guid? clientId, int skip, int take, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Contracts.AsNoTracking().AsQueryable();
        if (clientId.HasValue) query = query.Where(x => x.ClientId == clientId.Value);
        return await query.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id).Skip(skip).Take(take).ToListAsync(cancellationToken);
    }

    public Task<int> CountAsync(Guid? clientId, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Contracts.AsQueryable();
        if (clientId.HasValue) query = query.Where(x => x.ClientId == clientId.Value);
        return query.CountAsync(cancellationToken);
    }

    public async Task AddAsync(Contract contract, CancellationToken cancellationToken = default)
    {
        await dbContext.Contracts.AddAsync(contract, cancellationToken);
    }
}

internal sealed class ProjectRepository(ArchiCoreDbContext dbContext) : IProjectRepository
{
    public Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => dbContext.Projects.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<Project?> GetByContractAndCodeAsync(Guid contractId, ProjectCode projectCode, CancellationToken cancellationToken = default)
        => dbContext.Projects.SingleOrDefaultAsync(x => x.ContractId == contractId && x.ProjectCode == projectCode, cancellationToken);

    public async Task<IReadOnlyList<Project>> ListAsync(Guid? contractId, int skip, int take, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Projects.AsNoTracking().AsQueryable();
        if (contractId.HasValue) query = query.Where(x => x.ContractId == contractId.Value);
        return await query.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id).Skip(skip).Take(take).ToListAsync(cancellationToken);
    }

    public Task<int> CountAsync(Guid? contractId, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Projects.AsQueryable();
        if (contractId.HasValue) query = query.Where(x => x.ContractId == contractId.Value);
        return query.CountAsync(cancellationToken);
    }

    public async Task AddAsync(Project project, CancellationToken cancellationToken = default)
    {
        await dbContext.Projects.AddAsync(project, cancellationToken);
    }
}

internal sealed class WarehouseRepository(ArchiCoreDbContext dbContext) : IWarehouseRepository
{
    public Task<Warehouse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => dbContext.Warehouses.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<Warehouse?> GetByCodeAsync(WarehouseCode warehouseCode, CancellationToken cancellationToken = default)
        => dbContext.Warehouses.SingleOrDefaultAsync(x => x.WarehouseCode == warehouseCode, cancellationToken);

    public async Task<IReadOnlyList<Warehouse>> ListAsync(int skip, int take, CancellationToken cancellationToken = default)
        => await dbContext.Warehouses.AsNoTracking().OrderBy(x => x.CreatedAt).ThenBy(x => x.Id)
            .Skip(skip).Take(take).ToListAsync(cancellationToken);

    public Task<int> CountAsync(CancellationToken cancellationToken = default)
        => dbContext.Warehouses.CountAsync(cancellationToken);

    public async Task AddAsync(Warehouse warehouse, CancellationToken cancellationToken = default)
    {
        await dbContext.Warehouses.AddAsync(warehouse, cancellationToken);
    }


    public void TrackLocationBoxReference(Warehouse warehouse, Guid locationId, Guid boxId, bool occupied)
    {
        var location = warehouse.Locations.SingleOrDefault(item => item.Id == locationId)
            ?? throw new InvalidOperationException("The location does not belong to the supplied warehouse.");

        var trackedEntry = dbContext.ChangeTracker.Entries<WarehouseLocationBoxReference>()
            .SingleOrDefault(entry =>
                entry.Entity.BoxId == boxId &&
                entry.Property<Guid>("WarehouseId").CurrentValue == warehouse.Id &&
                entry.Property<Guid>("LocationId").CurrentValue == locationId);

        if (!occupied)
        {
            if (trackedEntry is not null)
                trackedEntry.State = EntityState.Deleted;
            return;
        }

        var reference = location.BoxReferences.SingleOrDefault(item => item.BoxId == boxId)
            ?? throw new InvalidOperationException("The domain aggregate does not contain the requested box occupancy.");

        var entry = trackedEntry is not null && ReferenceEquals(trackedEntry.Entity, reference)
            ? trackedEntry
            : dbContext.Entry(reference);

        if (entry.State != EntityState.Added)
        {
            entry.Property<Guid>("WarehouseId").CurrentValue = warehouse.Id;
            entry.Property<Guid>("LocationId").CurrentValue = locationId;
            entry.State = EntityState.Added;
        }
    }
}
