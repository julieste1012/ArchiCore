using ArchiCore.Domain.Projects;
using ArchiCore.Domain.ValueObjects;

namespace ArchiCore.Application.Abstractions.Persistence;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Project?> GetByContractAndCodeAsync(Guid contractId, ProjectCode projectCode, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Project>> ListAsync(Guid? contractId, int skip, int take, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid? contractId, CancellationToken cancellationToken = default);
    Task AddAsync(Project project, CancellationToken cancellationToken = default);
}
