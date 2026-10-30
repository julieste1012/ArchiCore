using ArchiCore.Domain.Boxes;
using ArchiCore.Domain.ValueObjects;

namespace ArchiCore.Application.Abstractions.Persistence;

public interface IBoxRepository
{
    Task<Box?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Box?> GetByCodeAsync(BoxCode boxCode, CancellationToken cancellationToken = default);
    Task<Box?> GetByBarcodeAsync(Barcode barcode, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Box>> ListAsync(
        BoxStatus? status,
        Barcode? barcode,
        int skip,
        int take,
        CancellationToken cancellationToken = default);
    Task<int> CountAsync(
        BoxStatus? status,
        Barcode? barcode,
        CancellationToken cancellationToken = default);
    Task AddAsync(Box box, CancellationToken cancellationToken = default);
}
