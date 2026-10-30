using Microsoft.AspNetCore.Authorization;
using ArchiCore.Application.Authentication;
using ArchiCore.API.Authorization;
using System.ComponentModel.DataAnnotations;
using ArchiCore.Application.Projects;
using Microsoft.AspNetCore.Mvc;

namespace ArchiCore.API.Controllers;

[Authorize]
[Route("api/projects")]
public sealed class ProjectsController(ProjectService service) : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = AppPolicies.ProjectsRead)]
    public async Task<IActionResult> List(
        [FromQuery] Guid? contractId,
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 20,
        CancellationToken cancellationToken = default)
        => Ok(await service.ListAsync(contractId, page, pageSize, cancellationToken));

    [HttpGet("{id:guid}")]
    [Authorize(Policy = AppPolicies.ProjectsRead)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        => ToResponse(await service.GetByIdAsync(id, cancellationToken));

    [HttpPost]
    [Authorize(Policy = AppPolicies.ProjectsOperate)]
    public async Task<IActionResult> Create(CreateProjectRequest request, CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(request, cancellationToken);
        return result.IsFailure
            ? ToProblem(result.Error)
            : CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
    }

    [HttpPost("{id:guid}/plan")]
    [Authorize(Policy = AppPolicies.ProjectsOperate)]
    public async Task<IActionResult> Plan(Guid id, CancellationToken cancellationToken)
        => ToResponse(await service.PlanAsync(id, cancellationToken));

    [HttpPost("{id:guid}/start")]
    [Authorize(Policy = AppPolicies.ProjectsOperate)]
    public async Task<IActionResult> Start(Guid id, CancellationToken cancellationToken)
        => ToResponse(await service.StartAsync(id, cancellationToken));

    [HttpPost("{id:guid}/suspend")]
    [Authorize(Policy = AppPolicies.ProjectsOperate)]
    public async Task<IActionResult> Suspend(Guid id, CancellationToken cancellationToken)
        => ToResponse(await service.SuspendAsync(id, cancellationToken));

    [HttpPost("{id:guid}/resume")]
    [Authorize(Policy = AppPolicies.ProjectsOperate)]
    public async Task<IActionResult> Resume(Guid id, CancellationToken cancellationToken)
        => ToResponse(await service.ResumeAsync(id, cancellationToken));

    [HttpPost("{id:guid}/complete")]
    [Authorize(Policy = AppPolicies.ProjectsOperate)]
    public async Task<IActionResult> Complete(Guid id, CancellationToken cancellationToken)
        => ToResponse(await service.CompleteAsync(id, cancellationToken));

    [HttpPost("{id:guid}/close")]
    [Authorize(Policy = AppPolicies.ProjectsOperate)]
    public async Task<IActionResult> Close(Guid id, CancellationToken cancellationToken)
        => ToResponse(await service.CloseAsync(id, cancellationToken));

    [HttpPost("{id:guid}/archive")]
    [Authorize(Policy = AppPolicies.ProjectsOperate)]
    public async Task<IActionResult> Archive(Guid id, CancellationToken cancellationToken)
        => ToResponse(await service.ArchiveAsync(id, cancellationToken));

    [HttpPost("{id:guid}/warehouses")]
    [Authorize(Policy = AppPolicies.ProjectsAssignWarehouses)]
    public async Task<IActionResult> AssignWarehouse(Guid id, AssignWarehouseRequest request, CancellationToken cancellationToken)
        => ToResponse(await service.AssignWarehouseAsync(id, request, cancellationToken));

    [HttpPost("{id:guid}/warehouses/release")]
    [Authorize(Policy = AppPolicies.ProjectsAssignWarehouses)]
    public async Task<IActionResult> ReleaseWarehouse(Guid id, ReleaseWarehouseRequest request, CancellationToken cancellationToken)
        => ToResponse(await service.ReleaseWarehouseAsync(id, request, cancellationToken));
}
