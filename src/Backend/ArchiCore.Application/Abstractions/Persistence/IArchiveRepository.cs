using ArchiCore.Domain.Archives;

namespace ArchiCore.Application.Abstractions.Persistence;

public interface IArchiveRepository
{
    Task<Archive?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Archive?> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task AddAsync(Archive archive, CancellationToken cancellationToken = default);
}
