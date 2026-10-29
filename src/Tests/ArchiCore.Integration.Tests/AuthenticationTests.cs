using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ArchiCore.Application.Authentication;
using ArchiCore.Integration.Tests.Infrastructure;
using Xunit;

namespace ArchiCore.Integration.Tests;

[Collection(IntegrationCollection.Name)]
public sealed class AuthenticationTests(ApiFixture fixture) : ApiTestBase(fixture)
{
    private const string StrongPassword = "Test_Strong_Password_2026!";

    [Fact]
    public async Task Login_returns_access_and_refresh_tokens_and_me_returns_the_authenticated_account()
    {
        using var anonymous = Fixture.CreateAnonymousClient();
        var user = await CreateUserAsync(anonymous, AppRoles.SystemAdministrator);

        using var login = await anonymous.PostAsJsonAsync("/api/auth/login",
            new { email = user.Email, password = StrongPassword }, Ct);
        await AssertStatusAsync(HttpStatusCode.OK, login);

        var session = await ReadBodyAsync<AuthenticationResponse>(login);
        Assert.False(string.IsNullOrWhiteSpace(session.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(session.RefreshToken));
        Assert.True(session.AccessTokenExpiresAtUtc > DateTime.UtcNow);
        Assert.True(session.RefreshTokenExpiresAtUtc > DateTime.UtcNow);

        anonymous.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);
        using var me = await anonymous.GetAsync("/api/auth/me", Ct);
        await AssertStatusAsync(HttpStatusCode.OK, me);
        var currentUser = await ReadBodyAsync<UserAccountDto>(me);
        Assert.Equal(user.Id, currentUser.Id);
        Assert.Equal(user.Email, currentUser.Email);
        Assert.Equal(AppRoles.SystemAdministrator, currentUser.Role);
    }

    [Fact]
    public async Task Wrong_password_returns_generic_401_without_disclosing_user_existence()
    {
        using var anonymous = Fixture.CreateAnonymousClient();
        var user = await CreateUserAsync(anonymous, AppRoles.SystemAdministrator);

        using var wrong = await anonymous.PostAsJsonAsync("/api/auth/login",
            new { email = user.Email, password = "Wrong_password_2026!" }, Ct);
        using var missing = await anonymous.PostAsJsonAsync("/api/auth/login",
            new { email = $"missing-{Guid.NewGuid():N}@archicore.test", password = "Wrong_password_2026!" }, Ct);

        await AssertStatusAsync(HttpStatusCode.Unauthorized, wrong);
        await AssertStatusAsync(HttpStatusCode.Unauthorized, missing);
        var wrongProblem = await ReadBodyAsync<ProblemResponse>(wrong);
        var missingProblem = await ReadBodyAsync<ProblemResponse>(missing);
        Assert.Equal(wrongProblem.Code, missingProblem.Code);
    }

    [Fact]
    public async Task Five_failed_logins_temporarily_lock_the_account_even_when_the_correct_password_is_supplied()
    {
        using var anonymous = Fixture.CreateAnonymousClient();
        var user = await CreateUserAsync(anonymous, AppRoles.SystemAdministrator);

        for (var attempt = 0; attempt < 5; attempt++)
        {
            using var response = await anonymous.PostAsJsonAsync("/api/auth/login",
                new { email = user.Email, password = "wrong-password" }, Ct);
            await AssertStatusAsync(HttpStatusCode.Unauthorized, response);
        }

        using var locked = await anonymous.PostAsJsonAsync("/api/auth/login",
            new { email = user.Email, password = StrongPassword }, Ct);
        await AssertStatusAsync(HttpStatusCode.Unauthorized, locked);
    }

    [Fact]
    public async Task Refresh_rotates_token_and_rejects_replay_of_the_previous_token()
    {
        using var anonymous = Fixture.CreateAnonymousClient();
        var user = await CreateUserAsync(anonymous, AppRoles.SystemAdministrator);
        var firstSession = await LoginAsync(anonymous, user.Email);

        using var refreshed = await anonymous.PostAsJsonAsync("/api/auth/refresh",
            new { refreshToken = firstSession.RefreshToken }, Ct);
        await AssertStatusAsync(HttpStatusCode.OK, refreshed);
        var secondSession = await ReadBodyAsync<AuthenticationResponse>(refreshed);
        Assert.NotEqual(firstSession.RefreshToken, secondSession.RefreshToken);

        using var replay = await anonymous.PostAsJsonAsync("/api/auth/refresh",
            new { refreshToken = firstSession.RefreshToken }, Ct);
        await AssertStatusAsync(HttpStatusCode.Unauthorized, replay);

        using var nextRotation = await anonymous.PostAsJsonAsync("/api/auth/refresh",
            new { refreshToken = secondSession.RefreshToken }, Ct);
        await AssertStatusAsync(HttpStatusCode.OK, nextRotation);
    }

    [Fact]
    public async Task Logout_revokes_refresh_token()
    {
        using var anonymous = Fixture.CreateAnonymousClient();
        var user = await CreateUserAsync(anonymous, AppRoles.SystemAdministrator);
        var session = await LoginAsync(anonymous, user.Email);

        using var logout = await anonymous.PostAsJsonAsync("/api/auth/logout",
            new { refreshToken = session.RefreshToken }, Ct);
        await AssertStatusAsync(HttpStatusCode.NoContent, logout);

        using var refresh = await anonymous.PostAsJsonAsync("/api/auth/refresh",
            new { refreshToken = session.RefreshToken }, Ct);
        await AssertStatusAsync(HttpStatusCode.Unauthorized, refresh);
    }

    [Fact]
    public async Task Operational_endpoints_reject_anonymous_requests()
    {
        using var anonymous = Fixture.CreateAnonymousClient();
        using var response = await anonymous.GetAsync("/api/customers", Ct);
        await AssertStatusAsync(HttpStatusCode.Unauthorized, response);

        using var auditResponse = await anonymous.GetAsync("/api/audit/domain-events", Ct);
        await AssertStatusAsync(HttpStatusCode.Unauthorized, auditResponse);
    }

    [Fact]
    public async Task Non_administrator_cannot_create_users_or_access_core_operational_controllers()
    {
        using var anonymous = Fixture.CreateAnonymousClient();
        var user = await CreateUserAsync(anonymous, AppRoles.ArchiveTechnician);
        var session = await LoginAsync(anonymous, user.Email);
        anonymous.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);

        using var createUser = await anonymous.PostAsJsonAsync("/api/auth/users", new
        {
            email = $"blocked-{Guid.NewGuid():N}@archicore.test",
            displayName = "Blocked Account",
            password = StrongPassword,
            role = AppRoles.ArchiveAssistant
        }, Ct);
        await AssertStatusAsync(HttpStatusCode.Forbidden, createUser);

        using var readCustomers = await anonymous.GetAsync("/api/customers", Ct);
        await AssertStatusAsync(HttpStatusCode.Forbidden, readCustomers);
    }

    [Fact]
    public async Task User_creation_rejects_weak_password_and_duplicate_email()
    {
        using var anonymous = Fixture.CreateAnonymousClient();
        using var weak = await Client.PostAsJsonAsync("/api/auth/users", new
        {
            email = $"weak-{Guid.NewGuid():N}@archicore.test",
            displayName = "Weak Password User",
            password = "weak",
            role = AppRoles.ArchiveTechnician
        }, Ct);
        await AssertStatusAsync(HttpStatusCode.UnprocessableEntity, weak);
        var validationProblem = await ReadBodyAsync<ProblemResponse>(weak);
        Assert.Equal("Api.ValidationFailed", validationProblem.Code);

        var user = await CreateUserAsync(anonymous, AppRoles.ArchiveTechnician);
        using var duplicate = await Client.PostAsJsonAsync("/api/auth/users", new
        {
            email = user.Email,
            displayName = "Duplicate User",
            password = StrongPassword,
            role = AppRoles.ArchiveTechnician
        }, Ct);
        await AssertStatusAsync(HttpStatusCode.Conflict, duplicate);
    }

    private async Task<UserAccountDto> CreateUserAsync(HttpClient anonymous, string role)
    {
        var email = $"test-{Guid.NewGuid():N}@archicore.test";
        using var response = await Client.PostAsJsonAsync("/api/auth/users", new
        {
            email,
            displayName = "Integration Test User",
            password = StrongPassword,
            role
        }, Ct);
        await AssertStatusAsync(HttpStatusCode.Created, response);
        return await ReadBodyAsync<UserAccountDto>(response);
    }

    private static async Task<AuthenticationResponse> LoginAsync(HttpClient client, string email)
    {
        using var response = await client.PostAsJsonAsync("/api/auth/login",
            new { email, password = StrongPassword }, Ct);
        await AssertStatusAsync(HttpStatusCode.OK, response);
        return await ReadBodyAsync<AuthenticationResponse>(response);
    }
}
