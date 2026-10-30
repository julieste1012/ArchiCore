using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using ArchiCore.API.Authorization;
using ArchiCore.Application.Receptions;
using ArchiCore.Domain.DocumentaryReception;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArchiCore.API.Controllers;

[Authorize]
[Route("api/receptions")]
public sealed class DocumentaryReceptionsController(DocumentaryReceptionService service) : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = AppPolicies.ReceptionsRead)]
    public async Task<IActionResult> List(
        [FromQuery] Guid? contractId,
        [FromQuery] DocumentaryReceptionStatus? status,
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 20,
        CancellationToken cancellationToken = default)
        => Ok(await service.ListAsync(contractId, status, page, pageSize, cancellationToken));

    [HttpGet("{id:guid}")]
    [Authorize(Policy = AppPolicies.ReceptionsRead)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        => ToResponse(await service.GetByIdAsync(id, cancellationToken));

    [HttpPost]
    [Authorize(Policy = AppPolicies.ReceptionsReceive)]
    public async Task<IActionResult> Create(
        CreateDocumentaryReceptionRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
            return Unauthorized();

        var result = await service.CreateAsync(request, userId, cancellationToken);
        return result.IsFailure
            ? ToProblem(result.Error)
            : CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
    }

    [HttpPost("{id:guid}/inspection")]
    [Authorize(Policy = AppPolicies.ReceptionsInspect)]
    public async Task<IActionResult> RecordInspection(
        Guid id,
        RecordReceptionInspectionRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
            return Unauthorized();

        return ToResponse(await service.RecordInspectionAsync(id, request, userId, cancellationToken));
    }

    [HttpPost("{id:guid}/counting")]
    [Authorize(Policy = AppPolicies.ReceptionsCount)]
    public async Task<IActionResult> RecordCounting(
        Guid id,
        RecordReceptionCountingRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
            return Unauthorized();

        return ToResponse(await service.RecordCountingAsync(id, request, userId, cancellationToken));
    }

    [HttpPost("{id:guid}/counting/approve")]
    [Authorize(Policy = AppPolicies.ReceptionsApprove)]
    public async Task<IActionResult> ApproveCounting(Guid id, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
            return Unauthorized();

        return ToResponse(await service.ApproveCountingAsync(id, userId, cancellationToken));
    }

    [HttpPost("{id:guid}/suspend")]
    [Authorize(Policy = AppPolicies.ReceptionsApprove)]
    public async Task<IActionResult> Suspend(
        Guid id,
        SuspendDocumentaryReceptionRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
            return Unauthorized();

        return ToResponse(await service.SuspendAsync(id, request.Reason, userId, cancellationToken));
    }

    private bool TryGetCurrentUserId(out Guid userId)
        => Guid.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out userId);
}
