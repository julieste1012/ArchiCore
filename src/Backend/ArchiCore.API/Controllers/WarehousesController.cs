using Microsoft.AspNetCore.Authorization;
using ArchiCore.Application.Authentication;
using ArchiCore.API.Authorization;
using System.ComponentModel.DataAnnotations;
using ArchiCore.Application.Warehouses;
using Microsoft.AspNetCore.Mvc;

namespace ArchiCore.API.Controllers;

[Authorize]
[Route("api/warehouses")]
public sealed class WarehousesController(WarehouseService service) : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = AppPolicies.WarehousesRead)]
    public async Task<IActionResult> List(
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 20,
        CancellationToken cancellationToken = default)
        => Ok(await service.ListAsync(page, pageSize, cancellationToken));

    [HttpGet("{id:guid}")]
    [Authorize(Policy = AppPolicies.WarehousesRead)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        => ToResponse(await service.GetByIdAsync(id, cancellationToken));

    [HttpPost]
    [Authorize(Policy = AppPolicies.WarehousesManage)]
    public async Task<IActionResult> Create(CreateWarehouseRequest request, CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(request, cancellationToken);
        return result.IsFailure
            ? ToProblem(result.Error)
            : CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
    }

    [HttpPost("{id:guid}/activate")]
    [Authorize(Policy = AppPolicies.WarehousesManage)]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
        => ToResponse(await service.ActivateAsync(id, cancellationToken));

    [HttpPost("{id:guid}/maintenance/start")]
    [Authorize(Policy = AppPolicies.WarehousesManage)]
    public async Task<IActionResult> StartMaintenance(Guid id, CancellationToken cancellationToken)
        => ToResponse(await service.StartMaintenanceAsync(id, cancellationToken));

    [HttpPost("{id:guid}/maintenance/finish")]
    [Authorize(Policy = AppPolicies.WarehousesManage)]
    public async Task<IActionResult> FinishMaintenance(Guid id, CancellationToken cancellationToken)
        => ToResponse(await service.FinishMaintenanceAsync(id, cancellationToken));

    [HttpPost("{id:guid}/close")]
    [Authorize(Policy = AppPolicies.WarehousesManage)]
    public async Task<IActionResult> Close(Guid id, CancellationToken cancellationToken)
        => ToResponse(await service.CloseAsync(id, cancellationToken));

    [HttpPost("{id:guid}/archive")]
    [Authorize(Policy = AppPolicies.WarehousesManage)]
    public async Task<IActionResult> Archive(Guid id, CancellationToken cancellationToken)
        => ToResponse(await service.ArchiveAsync(id, cancellationToken));
}
