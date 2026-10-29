using System.IdentityModel.Tokens.Jwt;
using ArchiCore.Application.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArchiCore.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthenticationController(IAuthenticationService authenticationService) : ApiControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await authenticationService.LoginAsync(request, cancellationToken);
        return result.IsFailure ? ToProblem(result.Error) : Ok(result.Value);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await authenticationService.RefreshAsync(request, cancellationToken);
        return result.IsFailure ? ToProblem(result.Error) : Ok(result.Value);
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        await authenticationService.RevokeRefreshTokenAsync(request, cancellationToken);
        return NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        var subject = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (!Guid.TryParse(subject, out var userId))
        {
            return Unauthorized();
        }

        var result = await authenticationService.GetUserAsync(userId, cancellationToken);
        return result.IsFailure ? ToProblem(result.Error) : Ok(result.Value);
    }

    [HttpPost("users")]
    [Authorize(Roles = AppRoles.SystemAdministrator)]
    public async Task<IActionResult> CreateUser(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var result = await authenticationService.CreateUserAsync(request, cancellationToken);
        return result.IsFailure
            ? ToProblem(result.Error)
            : StatusCode(StatusCodes.Status201Created, result.Value);
    }
}
