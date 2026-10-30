using ArchiCore.Application.Abstractions.Persistence;
using ArchiCore.Domain.Boxes;
using ArchiCore.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace ArchiCore.Infrastructure.Persistence.Repositories;

internal sealed class BoxRepository(ArchiCoreDbContext dbContext) : IBoxRepository
{
    public Task<Box?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => dbContext.Boxes.SingleOrDefaultAsync(box => box.Id == id, cancellationToken);

    public Task<Box?> GetByCodeAsync(BoxCode boxCode, CancellationToken cancellationToken = default)
        => dbContext.Boxes.SingleOrDefaultAsync(box => box.BoxCode == boxCode, cancellationToken);

    public Task<Box?> GetByBarcodeAsync(Barcode barcode, CancellationToken cancellationToken = default)
        => dbContext.Boxes.SingleOrDefaultAsync(box => box.Barcode == barcode, cancellationToken);

    public async Task<IReadOnlyList<Box>> ListAsync(
        BoxStatus? status,
        Barcode? barcode,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        var query = Filter(dbContext.Boxes.AsNoTracking(), status, barcode);
        return await query
            .OrderBy(box => box.CreatedAt)
            .ThenBy(box => box.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountAsync(
        BoxStatus? status,
        Barcode? barcode,
        CancellationToken cancellationToken = default)
        => Filter(dbContext.Boxes.AsNoTracking(), status, barcode).CountAsync(cancellationToken);

    public async Task AddAsync(Box box, CancellationToken cancellationToken = default)
        => await dbContext.Boxes.AddAsync(box, cancellationToken);

    private static IQueryable<Box> Filter(
        IQueryable<Box> query,
        BoxStatus? status,
        Barcode? barcode)
    {
        if (status.HasValue)
        {
            query = query.Where(box => box.Status == status.Value);
        }

        if (barcode is not null)
        {
            query = query.Where(box => box.Barcode == barcode);
        }

        return query;
    }
}
