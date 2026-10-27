using ArchiCore.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace ArchiCore.API.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult ToResponse<T>(Result<T> result)
        => result.IsSuccess ? Ok(result.Value) : ToProblem(result.Error);

    protected IActionResult ToProblem(Error error)
    {
        var code = error.Code;
        var status = IsNotFound(code) ? StatusCodes.Status404NotFound
            : IsConflict(code) ? StatusCodes.Status409Conflict
            : StatusCodes.Status422UnprocessableEntity;

        var problem = new ProblemDetails
        {
            Status = status,
            Title = code,
            Detail = error.Message,
            Instance = HttpContext.Request.Path
        };
        problem.Extensions["code"] = code;
        return StatusCode(status, problem);
    }

    private static bool IsNotFound(string code)
        => code.EndsWith(".NotFound", StringComparison.Ordinal)
           || code.EndsWith("CustomerNotFound", StringComparison.Ordinal)
           || code.EndsWith("ContractNotFound", StringComparison.Ordinal)
           || code.EndsWith("WarehouseNotFound", StringComparison.Ordinal);

    private static bool IsConflict(string code)
        => code.Contains("Duplicate", StringComparison.Ordinal)
           || code.Contains("InvalidActivation", StringComparison.Ordinal)
           || code.Contains("InvalidDeactivation", StringComparison.Ordinal)
           || code.Contains("InvalidArchival", StringComparison.Ordinal)
           || code.Contains("InvalidSuspension", StringComparison.Ordinal)
           || code.Contains("InvalidFinish", StringComparison.Ordinal)
           || code.Contains("InvalidPlanning", StringComparison.Ordinal)
           || code.Contains("InvalidStart", StringComparison.Ordinal)
           || code.Contains("InvalidResume", StringComparison.Ordinal)
           || code.Contains("InvalidCompletion", StringComparison.Ordinal)
           || code.Contains("InvalidClose", StringComparison.Ordinal)
           || code.Contains("InvalidMaintenance", StringComparison.Ordinal)
           || code.Contains("InvalidMove", StringComparison.Ordinal)
           || code.Contains("InvalidReturn", StringComparison.Ordinal)
           || code.Contains("InvalidArchive", StringComparison.Ordinal);
}
