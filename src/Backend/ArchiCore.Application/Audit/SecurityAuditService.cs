using ArchiCore.Application.Abstractions.Persistence;
using ArchiCore.Application.Common;
using ArchiCore.Domain.Common;

namespace ArchiCore.Application.Audit;

public sealed class SecurityAuditService(ISecurityAuditRepository repository)
{
    public async Task<Result<PagedResult<SecurityAuditDto>>> ListAsync(
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc,
        string? action,
        string? outcome,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (fromUtc.HasValue && toUtc.HasValue && fromUtc.Value > toUtc.Value)
        {
            return Result.Failure<PagedResult<SecurityAuditDto>>(
                new Error("Audit.InvalidDateRange", "fromUtc must be earlier than or equal to toUtc."));
        }

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var skipLong = ((long)page - 1L) * pageSize;
        var skip = (int)Math.Min(skipLong, int.MaxValue);
        var normalizedAction = string.IsNullOrWhiteSpace(action) ? null : action.Trim();
        var normalizedOutcome = string.IsNullOrWhiteSpace(outcome) ? null : outcome.Trim();

        var from = fromUtc?.UtcDateTime;
        var to = toUtc?.UtcDateTime;
        var items = await repository.ListAsync(
            from, to, normalizedAction, normalizedOutcome, skip, pageSize, cancellationToken);
        var totalCount = await repository.CountAsync(
            from, to, normalizedAction, normalizedOutcome, cancellationToken);

        return Result.Success(new PagedResult<SecurityAuditDto>(page, pageSize, totalCount, items));
    }
}
