using ArchiCore.Domain.Digitization;

namespace ArchiCore.Application.Abstractions.Persistence;

public interface IDigitizationRequestRepository
{
    Task<DigitizationRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DigitizationRequest>> ListByFolderAsync(Guid folderId, int skip, int take, CancellationToken cancellationToken = default);
    Task AddAsync(DigitizationRequest request, CancellationToken cancellationToken = default);
}
