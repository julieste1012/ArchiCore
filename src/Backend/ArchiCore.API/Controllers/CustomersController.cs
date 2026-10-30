using Microsoft.AspNetCore.Authorization;
using ArchiCore.Application.Authentication;
using ArchiCore.API.Authorization;
using System.ComponentModel.DataAnnotations;
using ArchiCore.Application.Customers;
using Microsoft.AspNetCore.Mvc;

namespace ArchiCore.API.Controllers;

[Authorize]
[Route("api/customers")]
public sealed class CustomersController(CustomerService service) : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = AppPolicies.CustomersRead)]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 20,
        CancellationToken cancellationToken = default)
        => Ok(await service.ListAsync(page, pageSize, cancellationToken));

    [HttpGet("{id:guid}")]
    [Authorize(Policy = AppPolicies.CustomersRead)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        => ToResponse(await service.GetByIdAsync(id, cancellationToken));

    [HttpPost]
    [Authorize(Policy = AppPolicies.CustomersManage)]
    public async Task<IActionResult> Create(CreateCustomerRequest request, CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(request, cancellationToken);
        return result.IsFailure
            ? ToProblem(result.Error)
            : CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
    }

    [HttpPost("{id:guid}/activate")]
    [Authorize(Policy = AppPolicies.CustomersManage)]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
        => ToResponse(await service.ActivateAsync(id, cancellationToken));

    [HttpPost("{id:guid}/deactivate")]
    [Authorize(Policy = AppPolicies.CustomersManage)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
        => ToResponse(await service.DeactivateAsync(id, cancellationToken));

    [HttpPost("{id:guid}/archive")]
    [Authorize(Policy = AppPolicies.CustomersManage)]
    public async Task<IActionResult> Archive(Guid id, CancellationToken cancellationToken)
        => ToResponse(await service.ArchiveAsync(id, cancellationToken));
}
