using System.ComponentModel.DataAnnotations;
using ArchiCore.Domain.Common;

namespace ArchiCore.Application.Authentication;

public sealed record LoginRequest(
    [property: Required, EmailAddress, MaxLength(254)] string Email,
    [property: Required, MaxLength(256)] string Password);

public sealed record RefreshTokenRequest(
    [property: Required, MaxLength(512)] string RefreshToken);

public sealed record CreateUserRequest(
    [property: Required, EmailAddress, MaxLength(254)] string Email,
    [property: Required, MinLength(2), MaxLength(120)] string DisplayName,
    [property: Required, MinLength(12), MaxLength(256)] string Password,
    [property: Required, MaxLength(64)] string Role);

public sealed record UserAccountDto(
    Guid Id,
    string Email,
    string DisplayName,
    string Role,
    bool IsActive,
    DateTime CreatedAtUtc);

public sealed record AuthenticationResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc,
    UserAccountDto User);

public interface IAuthenticationService
{
    Task<Result<AuthenticationResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<Result<AuthenticationResponse>> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);
    Task RevokeRefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);
    Task<Result<UserAccountDto>> GetUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<UserAccountDto>> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
}
