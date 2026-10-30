using Microsoft.AspNetCore.Authorization;
using ArchiCore.Application.Authentication;
using ArchiCore.API.Authorization;
using System.ComponentModel.DataAnnotations;
using ArchiCore.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace ArchiCore.API.Controllers;

[Authorize]
[Route("api/contracts")]
public sealed class ContractsController(ContractService service) : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = AppPolicies.ContractsRead)]
    public async Task<IActionResult> List(
        [FromQuery] Guid? clientId,
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 20,
        CancellationToken cancellationToken = default)
        => Ok(await service.ListAsync(clientId, page, pageSize, cancellationToken));

    [HttpGet("{id:guid}")]
    [Authorize(Policy = AppPolicies.ContractsRead)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        => ToResponse(await service.GetByIdAsync(id, cancellationToken));

    [HttpPost]
    [Authorize(Policy = AppPolicies.ContractsManage)]
    public async Task<IActionResult> Create(CreateContractRequest request, CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(request, cancellationToken);
        return result.IsFailure
            ? ToProblem(result.Error)
            : CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
    }

    [HttpPost("{id:guid}/activate")]
    [Authorize(Policy = AppPolicies.ContractsManage)]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
        => ToResponse(await service.ActivateAsync(id, cancellationToken));

    [HttpPost("{id:guid}/suspend")]
    [Authorize(Policy = AppPolicies.ContractsManage)]
    public async Task<IActionResult> Suspend(Guid id, CancellationToken cancellationToken)
        => ToResponse(await service.SuspendAsync(id, cancellationToken));

    [HttpPost("{id:guid}/finish")]
    [Authorize(Policy = AppPolicies.ContractsManage)]
    public async Task<IActionResult> Finish(Guid id, FinishContractRequest request, CancellationToken cancellationToken)
        => ToResponse(await service.FinishAsync(id, request.FinishDate, cancellationToken));
}
