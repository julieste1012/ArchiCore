using ArchiCore.Domain.Folders;
using ArchiCore.Domain.ValueObjects;

namespace ArchiCore.Application.Abstractions.Persistence;

public interface IFolderRepository
{
    Task<Folder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Folder?> GetByCodeAsync(FolderCode code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Folder>> ListByArchiveAsync(Guid archiveId, int skip, int take, CancellationToken cancellationToken = default);
    Task<int> CountByArchiveAsync(Guid archiveId, CancellationToken cancellationToken = default);
    Task AddAsync(Folder folder, CancellationToken cancellationToken = default);
}
