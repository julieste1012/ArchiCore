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
    public async Task Concurrent_refresh_requests_can_redeem_a_refresh_token_only_once()
    {
        using var setupClient = Fixture.CreateAnonymousClient();
        var user = await CreateUserAsync(setupClient, AppRoles.SystemAdministrator);
        var originalSession = await LoginAsync(setupClient, user.Email);

        using var clientA = Fixture.CreateAnonymousClient();
        using var clientB = Fixture.CreateAnonymousClient();

        // Both requests start from the same unconsumed refresh token. The row concurrency
        // token plus transaction must allow exactly one rotation to commit.
        var responses = await Task.WhenAll(
            clientA.PostAsJsonAsync("/api/auth/refresh",
                new { refreshToken = originalSession.RefreshToken }, Ct),
            clientB.PostAsJsonAsync("/api/auth/refresh",
                new { refreshToken = originalSession.RefreshToken }, Ct));

        using var responseA = responses[0];
        using var responseB = responses[1];

        var successful = responses.Where(response => response.StatusCode == HttpStatusCode.OK).ToArray();
        var rejected = responses.Where(response => response.StatusCode == HttpStatusCode.Unauthorized).ToArray();

        Assert.Single(successful);
        Assert.Single(rejected);

        var rotated = await ReadBodyAsync<AuthenticationResponse>(successful[0]);
        Assert.NotEqual(originalSession.RefreshToken, rotated.RefreshToken);

        using var replay = await clientA.PostAsJsonAsync("/api/auth/refresh",
            new { refreshToken = originalSession.RefreshToken }, Ct);
        await AssertStatusAsync(HttpStatusCode.Unauthorized, replay);
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

    [Fact]
    public async Task Administrator_can_list_users_and_the_response_is_paged()
    {
        var email = $"list-{Guid.NewGuid():N}@archicore.test";
        using var created = await Client.PostAsJsonAsync("/api/auth/users", new
        {
            email,
            displayName = "List User",
            password = StrongPassword,
            role = AppRoles.ArchiveTechnician
        }, Ct);
        await AssertStatusAsync(HttpStatusCode.Created, created);

        using var page = await Client.GetAsync("/api/auth/users?page=1&pageSize=1", Ct);
        await AssertStatusAsync(HttpStatusCode.OK, page);
        var result = await ReadBodyAsync<PagedResponse<UserAccountDto>>(page);
        Assert.Single(result.Items);
        Assert.True(result.TotalCount >= 2);
    }

    [Fact]
    public async Task Deactivation_revokes_refresh_sessions_and_invalidates_existing_access_tokens_even_after_reactivation()
    {
        using var anonymous = Fixture.CreateAnonymousClient();
        var account = await CreateUserAsync(anonymous, AppRoles.ArchiveTechnician);
        var session = await LoginAsync(anonymous, account.Email);
        anonymous.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);

        using var deactivated = await Client.PutAsJsonAsync($"/api/auth/users/{account.Id}/status", new { IsActive = false }, Ct);
        await AssertStatusAsync(HttpStatusCode.OK, deactivated);
        var disabled = await ReadBodyAsync<UserAccountDto>(deactivated);
        Assert.False(disabled.IsActive);

        using var oldAccess = await anonymous.GetAsync("/api/auth/me", Ct);
        await AssertStatusAsync(HttpStatusCode.Unauthorized, oldAccess);

        anonymous.DefaultRequestHeaders.Authorization = null;
        using var oldRefresh = await anonymous.PostAsJsonAsync("/api/auth/refresh", new { refreshToken = session.RefreshToken }, Ct);
        await AssertStatusAsync(HttpStatusCode.Unauthorized, oldRefresh);

        using var reactivated = await Client.PutAsJsonAsync($"/api/auth/users/{account.Id}/status", new { IsActive = true }, Ct);
        await AssertStatusAsync(HttpStatusCode.OK, reactivated);
        var enabled = await ReadBodyAsync<UserAccountDto>(reactivated);
        Assert.True(enabled.IsActive);

        anonymous.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);
        using var staleAccess = await anonymous.GetAsync("/api/auth/me", Ct);
        await AssertStatusAsync(HttpStatusCode.Unauthorized, staleAccess);

        anonymous.DefaultRequestHeaders.Authorization = null;
        var newSession = await LoginAsync(anonymous, account.Email);
        Assert.NotEqual(session.AccessToken, newSession.AccessToken);
    }

    [Fact]
    public async Task Administrator_cannot_deactivate_their_own_account()
    {
        using var meResponse = await Client.GetAsync("/api/auth/me", Ct);
        await AssertStatusAsync(HttpStatusCode.OK, meResponse);
        var me = await ReadBodyAsync<UserAccountDto>(meResponse);

        using var response = await Client.PutAsJsonAsync($"/api/auth/users/{me.Id}/status", new { IsActive = false }, Ct);

        await AssertStatusAsync(HttpStatusCode.Conflict, response);
        var problem = await ReadBodyAsync<ProblemResponse>(response);
        Assert.Equal("Authentication.CannotDeactivateSelf", problem.Code);
    }

    [Fact]
    public async Task Non_administrator_cannot_list_or_deactivate_users()
    {
        using var anonymous = Fixture.CreateAnonymousClient();
        var user = await CreateUserAsync(anonymous, AppRoles.ArchiveTechnician);
        var session = await LoginAsync(anonymous, user.Email);
        anonymous.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);

        using var list = await anonymous.GetAsync("/api/auth/users", Ct);
        await AssertStatusAsync(HttpStatusCode.Forbidden, list);

        using var deactivate = await anonymous.PutAsJsonAsync($"/api/auth/users/{user.Id}/status", new { IsActive = false }, Ct);
        await AssertStatusAsync(HttpStatusCode.Forbidden, deactivate);
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
