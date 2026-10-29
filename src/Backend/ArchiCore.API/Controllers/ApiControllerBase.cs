using ArchiCore.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace ArchiCore.API.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    private static readonly HashSet<string> NotFoundErrorCodes = new(StringComparer.Ordinal)
    {
        "Customer.NotFound",
        "Customer.ContractNotFound",
        "Contract.NotFound",
        "Contract.CustomerNotFound",
        "Contract.ProjectNotFound",
        "Project.NotFound",
        "Project.ContractNotFound",
        "Project.WarehouseNotFound",
        "Project.WarehouseAssignmentNotFound",
        "Warehouse.NotFound",
        "Authentication.UserNotFound",
    };

    private static readonly HashSet<string> UnauthorizedErrorCodes = new(StringComparer.Ordinal)
    {
        "Authentication.InvalidCredentials",
        "Authentication.InvalidRefreshToken"
    };

    private static readonly HashSet<string> ConflictErrorCodes = new(StringComparer.Ordinal)
    {
        "Customer.DuplicateNit",
        "Customer.DuplicateContract",
        "Customer.InvalidActivation",
        "Customer.InvalidDeactivation",
        "Customer.InvalidArchival",
        "Contract.DuplicateNumber",
        "Contract.DuplicateProjectCode",
        "Contract.InvalidActivation",
        "Contract.InvalidSuspension",
        "Contract.InvalidFinish",
        "Project.DuplicateProjectCode",
        "Project.DuplicateWarehouseAssignment",
        "Project.InvalidPlanning",
        "Project.InvalidStart",
        "Project.InvalidSuspension",
        "Project.InvalidResume",
        "Project.InvalidCompletion",
        "Project.InvalidClose",
        "Project.InvalidArchival",
        "Warehouse.DuplicateCode",
        "Warehouse.InvalidActivation",
        "Warehouse.InvalidMaintenance",
        "Warehouse.InvalidClose",
        "Warehouse.InvalidArchive",
        "Authentication.EmailAlreadyExists",
        "Authentication.CannotDeactivateSelf",
        "Authentication.CannotDeactivateLastAdministrator",
        "Box.InvalidMove",
        "Box.InvalidReturn",
        "Box.InvalidArchive"
    };

    protected IActionResult ToResponse<T>(Result<T> result)
        => result.IsSuccess ? Ok(result.Value) : ToProblem(result.Error);

    protected IActionResult ToProblem(Error error)
    {
        var status = ResolveStatusCode(error.Code);
        var problem = new ProblemDetails
        {
            Status = status,
            Title = error.Code,
            Detail = error.Message,
            Instance = HttpContext.Request.Path
        };
        problem.Extensions["code"] = error.Code;
        return StatusCode(status, problem);
    }

    private static int ResolveStatusCode(string code)
    {
        if (NotFoundErrorCodes.Contains(code))
        {
            return StatusCodes.Status404NotFound;
        }

        if (UnauthorizedErrorCodes.Contains(code))
        {
            return StatusCodes.Status401Unauthorized;
        }

        if (ConflictErrorCodes.Contains(code))
        {
            return StatusCodes.Status409Conflict;
        }

        return StatusCodes.Status422UnprocessableEntity;
    }
}
