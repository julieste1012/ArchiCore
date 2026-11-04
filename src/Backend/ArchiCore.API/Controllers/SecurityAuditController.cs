using System.ComponentModel.DataAnnotations;
using ArchiCore.Application.Audit;
using ArchiCore.Application.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArchiCore.API.Controllers;

[Authorize(Roles = AppRoles.SystemAdministrator)]
[Route("api/audit/security-events")]
public sealed class SecurityAuditController(SecurityAuditService service) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] DateTimeOffset? fromUtc,
        [FromQuery] DateTimeOffset? toUtc,
        [FromQuery, MaxLength(128)] string? action,
        [FromQuery, MaxLength(32)] string? outcome,
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 20,
        CancellationToken cancellationToken = default)
        => ToResponse(await service.ListAsync(
            fromUtc, toUtc, action, outcome, page, pageSize, cancellationToken));
}
