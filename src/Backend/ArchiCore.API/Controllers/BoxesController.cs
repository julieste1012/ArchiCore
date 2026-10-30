using System.ComponentModel.DataAnnotations;
using ArchiCore.API.Authorization;
using ArchiCore.Application.Boxes;
using ArchiCore.Domain.Boxes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArchiCore.API.Controllers;

[Authorize]
[Route("api/boxes")]
public sealed class BoxesController(BoxService service) : ApiControllerBase
{
    [HttpGet]
    [Authorize(Policy = AppPolicies.BoxesRead)]
    public async Task<IActionResult> List(
        [FromQuery] BoxStatus? status,
        [FromQuery, MaxLength(100)] string? barcode,
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 20,
        CancellationToken cancellationToken = default)
        => ToResponse(await service.ListAsync(status, barcode, page, pageSize, cancellationToken));

    [HttpGet("{id:guid}")]
    [Authorize(Policy = AppPolicies.BoxesRead)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        => ToResponse(await service.GetByIdAsync(id, cancellationToken));

    [HttpPost]
    [Authorize(Policy = AppPolicies.BoxesRegister)]
    public async Task<IActionResult> Create(
        CreateBoxRequest request,
        CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(request, cancellationToken);
        return result.IsFailure
            ? ToProblem(result.Error)
            : CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
    }

    [HttpPost("{id:guid}/barcode")]
    [Authorize(Policy = AppPolicies.BoxesRegister)]
    public async Task<IActionResult> AssignBarcode(
        Guid id,
        AssignBoxBarcodeRequest request,
        CancellationToken cancellationToken)
        => ToResponse(await service.AssignBarcodeAsync(id, request, cancellationToken));
}
