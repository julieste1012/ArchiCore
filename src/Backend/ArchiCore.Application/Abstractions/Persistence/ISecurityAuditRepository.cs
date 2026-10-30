using ArchiCore.Application.Audit;

namespace ArchiCore.Application.Abstractions.Persistence;

public interface ISecurityAuditRepository
{
    Task<IReadOnlyList<SecurityAuditDto>> ListAsync(
        DateTime? fromUtc,
        DateTime? toUtc,
        string? action,
        string? outcome,
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    Task<int> CountAsync(
        DateTime? fromUtc,
        DateTime? toUtc,
        string? action,
        string? outcome,
        CancellationToken cancellationToken = default);
}
