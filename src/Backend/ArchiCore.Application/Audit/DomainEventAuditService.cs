using ArchiCore.Application.Abstractions.Persistence;
using ArchiCore.Application.Common;
using ArchiCore.Domain.Common;

namespace ArchiCore.Application.Audit;

public sealed class DomainEventAuditService(IDomainEventAuditRepository auditRepository)
{
    public async Task<Result<PagedResult<DomainEventAuditDto>>> ListAsync(
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc,
        string? eventType,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (fromUtc.HasValue && toUtc.HasValue && fromUtc.Value > toUtc.Value)
        {
            return Result.Failure<PagedResult<DomainEventAuditDto>>(
                new Error("Audit.InvalidDateRange", "fromUtc must be earlier than or equal to toUtc."));
        }

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var normalizedEventType = string.IsNullOrWhiteSpace(eventType) ? null : eventType.Trim();
        var from = fromUtc?.UtcDateTime;
        var to = toUtc?.UtcDateTime;
        var skip = (page - 1) * pageSize;

        var items = await auditRepository.ListAsync(
            from, to, normalizedEventType, skip, pageSize, cancellationToken);
        var totalCount = await auditRepository.CountAsync(
            from, to, normalizedEventType, cancellationToken);

        return Result.Success(new PagedResult<DomainEventAuditDto>(page, pageSize, totalCount, items));
    }
}
