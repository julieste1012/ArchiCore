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

        anonymous.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", secondSession.AccessToken);
        using var staleAccess = await anonymous.GetAsync("/api/auth/me", Ct);
        await AssertStatusAsync(HttpStatusCode.Unauthorized, staleAccess);

        anonymous.DefaultRequestHeaders.Authorization = null;
        using var nextRotation = await anonymous.PostAsJsonAsync("/api/auth/refresh",
            new { refreshToken = secondSession.RefreshToken }, Ct);
        await AssertStatusAsync(HttpStatusCode.Unauthorized, nextRotation);
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
    public async Task Administrative_manager_can_manage_customers_and_read_contracts_but_cannot_administer_users()
    {
        using var anonymous = Fixture.CreateAnonymousClient();
        var user = await CreateUserAsync(anonymous, AppRoles.AdministrativeManager);
        var session = await LoginAsync(anonymous, user.Email);
        anonymous.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);

        using var customers = await anonymous.GetAsync("/api/customers", Ct);
        await AssertStatusAsync(HttpStatusCode.OK, customers);

        using var contracts = await anonymous.GetAsync("/api/contracts", Ct);
        await AssertStatusAsync(HttpStatusCode.OK, contracts);

        using var createCustomer = await anonymous.PostAsJsonAsync("/api/customers",
            new { nit = TestData.Nit() }, Ct);
        await AssertStatusAsync(HttpStatusCode.Created, createCustomer);

        using var createUser = await anonymous.PostAsJsonAsync("/api/auth/users", new
        {
            email = $"forbidden-{Guid.NewGuid():N}@archicore.test",
            displayName = "Forbidden User",
            password = StrongPassword,
            role = AppRoles.ArchiveAssistant
        }, Ct);
        await AssertStatusAsync(HttpStatusCode.Forbidden, createUser);

        using var audit = await anonymous.GetAsync("/api/audit/domain-events", Ct);
        await AssertStatusAsync(HttpStatusCode.Forbidden, audit);
    }

    [Fact]
    public async Task Project_coordinator_can_operate_projects_and_assign_warehouses()
    {
        var customer = await CreateCustomerAsync();
        var contract = await CreateContractAsync(customer.Id);
        var warehouse = await CreateWarehouseAsync();

        using var anonymous = Fixture.CreateAnonymousClient();
        var user = await CreateUserAsync(anonymous, AppRoles.ProjectCoordinator);
        var session = await LoginAsync(anonymous, user.Email);
        anonymous.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);

        using var projects = await anonymous.GetAsync("/api/projects", Ct);
        await AssertStatusAsync(HttpStatusCode.OK, projects);

        using var warehouses = await anonymous.GetAsync("/api/warehouses", Ct);
        await AssertStatusAsync(HttpStatusCode.OK, warehouses);

        using var createProject = await anonymous.PostAsJsonAsync("/api/projects",
            new { projectCode = TestData.ProjectCode(), contractId = contract.Id }, Ct);
        await AssertStatusAsync(HttpStatusCode.Created, createProject);
        var project = await ReadBodyAsync<ProjectResponse>(createProject);

        using var assign = await anonymous.PostAsJsonAsync($"/api/projects/{project.Id}/warehouses",
            new { warehouseId = warehouse.Id, assignedFrom = FixedDate }, Ct);
        await AssertStatusAsync(HttpStatusCode.OK, assign);

        using var createCustomer = await anonymous.PostAsJsonAsync("/api/customers",
            new { nit = TestData.Nit() }, Ct);
        await AssertStatusAsync(HttpStatusCode.Forbidden, createCustomer);
    }

    [Fact]
    public async Task Archive_assistant_can_read_warehouse_registry_but_cannot_administer_warehouses_or_projects()
    {
        using var anonymous = Fixture.CreateAnonymousClient();
        var user = await CreateUserAsync(anonymous, AppRoles.ArchiveAssistant);
        var session = await LoginAsync(anonymous, user.Email);
        anonymous.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);

        using var warehouses = await anonymous.GetAsync("/api/warehouses", Ct);
        await AssertStatusAsync(HttpStatusCode.OK, warehouses);

        using var createWarehouse = await anonymous.PostAsJsonAsync("/api/warehouses",
            new { warehouseCode = TestData.WarehouseCode() }, Ct);
        await AssertStatusAsync(HttpStatusCode.Forbidden, createWarehouse);

        using var projects = await anonymous.GetAsync("/api/projects", Ct);
        await AssertStatusAsync(HttpStatusCode.OK, projects);
    }

    [Fact]
    public async Task Archive_technician_and_digitization_operator_can_read_project_context_without_operational_write_access()
    {
        using var anonymous = Fixture.CreateAnonymousClient();
        var technician = await CreateUserAsync(anonymous, AppRoles.ArchiveTechnician);
        var technicianSession = await LoginAsync(anonymous, technician.Email);
        anonymous.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", technicianSession.AccessToken);

        using var technicianProjects = await anonymous.GetAsync("/api/projects", Ct);
        await AssertStatusAsync(HttpStatusCode.OK, technicianProjects);

        using var technicianWarehouses = await anonymous.GetAsync("/api/warehouses", Ct);
        await AssertStatusAsync(HttpStatusCode.OK, technicianWarehouses);

        using var technicianCreateProject = await anonymous.PostAsJsonAsync("/api/projects", new
        {
            projectCode = TestData.ProjectCode(),
            contractId = Guid.NewGuid()
        }, Ct);
        await AssertStatusAsync(HttpStatusCode.Forbidden, technicianCreateProject);

        anonymous.DefaultRequestHeaders.Authorization = null;
        var operatorAccount = await CreateUserAsync(anonymous, AppRoles.DigitizationOperator);
        var operatorSession = await LoginAsync(anonymous, operatorAccount.Email);
        anonymous.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", operatorSession.AccessToken);

        using var operatorProjects = await anonymous.GetAsync("/api/projects", Ct);
        await AssertStatusAsync(HttpStatusCode.OK, operatorProjects);

        using var operatorWarehouses = await anonymous.GetAsync("/api/warehouses", Ct);
        await AssertStatusAsync(HttpStatusCode.Forbidden, operatorWarehouses);
    }

    [Fact]
    public async Task Billing_assistant_can_read_contracts_and_projects_without_lifecycle_permissions()
    {
        using var anonymous = Fixture.CreateAnonymousClient();
        var user = await CreateUserAsync(anonymous, AppRoles.BillingAssistant);
        var session = await LoginAsync(anonymous, user.Email);
        anonymous.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);

        using var contracts = await anonymous.GetAsync("/api/contracts", Ct);
        await AssertStatusAsync(HttpStatusCode.OK, contracts);

        using var projects = await anonymous.GetAsync("/api/projects", Ct);
        await AssertStatusAsync(HttpStatusCode.OK, projects);

        using var createContract = await anonymous.PostAsJsonAsync("/api/contracts", new
        {
            contractNumber = TestData.ContractNumber(),
            clientId = Guid.NewGuid(),
            startDate = FixedDate,
            totalValue = 1000m,
            currency = "COP"
        }, Ct);
        await AssertStatusAsync(HttpStatusCode.Forbidden, createContract);
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
    public async Task Changing_password_revokes_old_access_and_refresh_tokens_and_allows_login_with_the_new_password()
    {
        using var anonymous = Fixture.CreateAnonymousClient();
        var account = await CreateUserAsync(anonymous, AppRoles.ArchiveTechnician);
        var session = await LoginAsync(anonymous, account.Email);
        anonymous.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);

        using var change = await anonymous.PostAsJsonAsync("/api/auth/change-password", new
        {
            currentPassword = StrongPassword,
            newPassword = "Another_Strong_Password_2026!"
        }, Ct);
        await AssertStatusAsync(HttpStatusCode.NoContent, change);

        using var oldAccess = await anonymous.GetAsync("/api/auth/me", Ct);
        await AssertStatusAsync(HttpStatusCode.Unauthorized, oldAccess);

        anonymous.DefaultRequestHeaders.Authorization = null;
        using var oldRefresh = await anonymous.PostAsJsonAsync("/api/auth/refresh",
            new { refreshToken = session.RefreshToken }, Ct);
        await AssertStatusAsync(HttpStatusCode.Unauthorized, oldRefresh);

        using var oldLogin = await anonymous.PostAsJsonAsync("/api/auth/login",
            new { email = account.Email, password = StrongPassword }, Ct);
        await AssertStatusAsync(HttpStatusCode.Unauthorized, oldLogin);

        var newSession = await LoginAsync(anonymous, account.Email, "Another_Strong_Password_2026!");
        Assert.False(string.IsNullOrWhiteSpace(newSession.AccessToken));
    }

    [Fact]
    public async Task Changing_password_rejects_an_incorrect_current_password_without_revoking_the_session()
    {
        using var anonymous = Fixture.CreateAnonymousClient();
        var account = await CreateUserAsync(anonymous, AppRoles.ArchiveTechnician);
        var session = await LoginAsync(anonymous, account.Email);
        anonymous.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);

        using var response = await anonymous.PostAsJsonAsync("/api/auth/change-password", new
        {
            currentPassword = "Incorrect_Current_Password_2026!",
            newPassword = "Another_Strong_Password_2026!"
        }, Ct);
        await AssertStatusAsync(HttpStatusCode.Unauthorized, response);
        var problem = await ReadBodyAsync<ProblemResponse>(response);
        Assert.Equal("Authentication.InvalidCurrentPassword", problem.Code);

        using var me = await anonymous.GetAsync("/api/auth/me", Ct);
        await AssertStatusAsync(HttpStatusCode.OK, me);

        anonymous.DefaultRequestHeaders.Authorization = null;
        var stillWorks = await LoginAsync(anonymous, account.Email, StrongPassword);
        Assert.False(string.IsNullOrWhiteSpace(stillWorks.AccessToken));
    }

    [Fact]
    public async Task Change_password_requires_authentication_and_rejects_reusing_the_current_password()
    {
        using var anonymous = Fixture.CreateAnonymousClient();

        using var unauthenticated = await anonymous.PostAsJsonAsync("/api/auth/change-password", new
        {
            currentPassword = StrongPassword,
            newPassword = "Another_Strong_Password_2026!"
        }, Ct);
        await AssertStatusAsync(HttpStatusCode.Unauthorized, unauthenticated);

        var account = await CreateUserAsync(anonymous, AppRoles.ArchiveTechnician);
        var session = await LoginAsync(anonymous, account.Email);
        anonymous.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);

        using var samePassword = await anonymous.PostAsJsonAsync("/api/auth/change-password", new
        {
            currentPassword = StrongPassword,
            newPassword = StrongPassword
        }, Ct);
        await AssertStatusAsync(HttpStatusCode.UnprocessableEntity, samePassword);
        var problem = await ReadBodyAsync<ProblemResponse>(samePassword);
        Assert.Equal("Authentication.NewPasswordMustDiffer", problem.Code);
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

    private static async Task<AuthenticationResponse> LoginAsync(
        HttpClient client,
        string email,
        string password = StrongPassword)
    {
        using var response = await client.PostAsJsonAsync("/api/auth/login",
            new { email, password }, Ct);
        await AssertStatusAsync(HttpStatusCode.OK, response);
        return await ReadBodyAsync<AuthenticationResponse>(response);
    }
}
