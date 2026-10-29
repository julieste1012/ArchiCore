using System.ComponentModel.DataAnnotations;
using ArchiCore.Application.Common;
using ArchiCore.Domain.Common;

namespace ArchiCore.Application.Authentication;

public sealed record LoginRequest(
    [param: Required, EmailAddress, MaxLength(254)] string Email,
    [param: Required, MaxLength(256)] string Password);

public sealed record RefreshTokenRequest(
    [param: Required, MaxLength(512)] string RefreshToken);

public sealed record CreateUserRequest(
    [param: Required, EmailAddress, MaxLength(254)] string Email,
    [param: Required, MinLength(2), MaxLength(120)] string DisplayName,
    [param: Required, MinLength(12), MaxLength(256)] string Password,
    [param: Required, MaxLength(64)] string Role);

public sealed record UserAccountDto(
    Guid Id,
    string Email,
    string DisplayName,
    string Role,
    bool IsActive,
    DateTime CreatedAtUtc);

public sealed record SetUserStatusRequest(bool IsActive);

public sealed record ChangePasswordRequest(
    [param: Required, MaxLength(256)] string CurrentPassword,
    [param: Required, MinLength(12), MaxLength(256)] string NewPassword);

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
    Task<Result<PagedResult<UserAccountDto>>> ListUsersAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<Result<UserAccountDto>> SetUserActiveAsync(Guid userId, bool isActive, CancellationToken cancellationToken = default);
    Task<Result> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);
}
