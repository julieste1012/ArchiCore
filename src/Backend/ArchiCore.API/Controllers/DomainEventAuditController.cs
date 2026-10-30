using System.ComponentModel.DataAnnotations;
using ArchiCore.Application.Audit;
using ArchiCore.Application.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArchiCore.API.Controllers;

[Authorize(Roles = AppRoles.SystemAdministrator)]
[Route("api/audit/domain-events")]
public sealed class DomainEventAuditController(DomainEventAuditService service) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] DateTimeOffset? fromUtc,
        [FromQuery] DateTimeOffset? toUtc,
        [FromQuery, MaxLength(512)] string? eventType,
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 20,
        CancellationToken cancellationToken = default)
        => ToResponse(await service.ListAsync(fromUtc, toUtc, eventType, page, pageSize, cancellationToken));
}
