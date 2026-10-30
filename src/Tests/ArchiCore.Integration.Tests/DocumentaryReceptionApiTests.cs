using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ArchiCore.Application.Authentication;
using ArchiCore.Integration.Tests.Infrastructure;
using Xunit;

namespace ArchiCore.Integration.Tests;

[Collection(IntegrationCollection.Name)]
public sealed class DocumentaryReceptionApiTests(ApiFixture fixture) : ApiTestBase(fixture)
{
    private const string StrongPassword = "Reception_Test_2026!Strong";

    [Fact]
    public async Task Coordinator_and_archive_assistant_can_execute_reception_counting_and_approval_workflow()
    {
        var customer = await CreateCustomerAsync();
        var contract = await CreateContractAsync(customer.Id);
        using (var activate = await PostAsync($"/api/contracts/{contract.Id}/activate", new { }))
            await AssertStatusAsync(HttpStatusCode.OK, activate);

        var coordinator = await CreateUserAsync(AppRoles.ProjectCoordinator);
        using var coordinatorClient = await LoginAsync(coordinator.Email);

        var assistant = await CreateUserAsync(AppRoles.ArchiveAssistant);
        using var assistantClient = await LoginAsync(assistant.Email);

        using var created = await coordinatorClient.PostAsJsonAsync("/api/receptions", new
        {
            contractId = contract.Id,
            initialObservations = "Entrega recibida con inventario firmado."
        }, Ct);
        await AssertStatusAsync(HttpStatusCode.Created, created);
        var reception = await ReadBodyAsync<DocumentaryReceptionResponse>(created);

        Assert.Equal(contract.Id, reception.ContractId);
        Assert.Equal(customer.Id, reception.CustomerId);
        Assert.Equal(coordinator.Id, reception.ReceivedByUserId);
        Assert.Equal("Received", reception.Status);

        using var inspection = await assistantClient.PostAsJsonAsync(
            $"/api/receptions/{reception.Id}/inspection",
            new { safeToAccept = true, observations = "Cajas íntegras, etiquetas legibles y sellos completos." }, Ct);
        await AssertStatusAsync(HttpStatusCode.OK, inspection);
        var inspected = await ReadBodyAsync<DocumentaryReceptionResponse>(inspection);
        Assert.Equal("Inspected", inspected.Status);
        Assert.Equal(assistant.Id, inspected.InspectedByUserId);
        Assert.True(inspected.SafeToAccept);

        using var counting = await assistantClient.PostAsJsonAsync(
            $"/api/receptions/{reception.Id}/counting",
            new
            {
                declaredBoxCount = 10,
                declaredFolderCount = 120,
                countedBoxCount = 9,
                countedFolderCount = 118,
                discrepancyReport = "Se identificó una caja y dos carpetas faltantes respecto del inventario de entrega."
            }, Ct);
        await AssertStatusAsync(HttpStatusCode.OK, counting);
        var counted = await ReadBodyAsync<DocumentaryReceptionResponse>(counting);
        Assert.Equal("CountingRecorded", counted.Status);
        Assert.Equal(assistant.Id, counted.CountedByUserId);
        Assert.Equal(10, counted.DeclaredBoxCount);
        Assert.Equal(9, counted.CountedBoxCount);

        using var assistantApproval = await assistantClient.PostAsync(
            $"/api/receptions/{reception.Id}/counting/approve", null, TestContext.Current.CancellationToken);
        await AssertStatusAsync(HttpStatusCode.Forbidden, assistantApproval);

        using var approved = await coordinatorClient.PostAsync(
            $"/api/receptions/{reception.Id}/counting/approve", null);
        await AssertStatusAsync(HttpStatusCode.OK, approved);
        var finalReception = await ReadBodyAsync<DocumentaryReceptionResponse>(approved);
        Assert.Equal("Counted", finalReception.Status);
        Assert.Equal(coordinator.Id, finalReception.ApprovedByUserId);

        using var list = await coordinatorClient.GetAsync(
            $"/api/receptions?contractId={contract.Id}&status=Counted&page=1&pageSize=20", Ct);
        await AssertStatusAsync(HttpStatusCode.OK, list);
        var results = await ReadBodyAsync<PagedResponse<DocumentaryReceptionResponse>>(list);
        Assert.Contains(results.Items, item => item.Id == reception.Id);
    }

    [Fact]
    public async Task Reception_requires_an_active_contract()
    {
        var customer = await CreateCustomerAsync();
        var draftContract = await CreateContractAsync(customer.Id);
        var coordinator = await CreateUserAsync(AppRoles.ProjectCoordinator);
        using var coordinatorClient = await LoginAsync(coordinator.Email);

        using var response = await coordinatorClient.PostAsJsonAsync("/api/receptions", new
        {
            contractId = draftContract.Id,
            initialObservations = "Entrega presentada para recepción."
        }, Ct);

        await AssertStatusAsync(HttpStatusCode.Conflict, response);
        var problem = await ReadBodyAsync<ProblemResponse>(response);
        Assert.Equal("Reception.ContractNotActive", problem.Code);
    }

    [Fact]
    public async Task Archive_assistant_cannot_create_or_approve_a_reception()
    {
        var customer = await CreateCustomerAsync();
        var contract = await CreateContractAsync(customer.Id);
        using (var activate = await PostAsync($"/api/contracts/{contract.Id}/activate", new { }))
            await AssertStatusAsync(HttpStatusCode.OK, activate);

        var assistant = await CreateUserAsync(AppRoles.ArchiveAssistant);
        using var assistantClient = await LoginAsync(assistant.Email);

        using var create = await assistantClient.PostAsJsonAsync("/api/receptions", new
        {
            contractId = contract.Id,
            initialObservations = "No autorizado a registrar recepción."
        }, Ct);
        await AssertStatusAsync(HttpStatusCode.Forbidden, create);
    }

    private async Task<UserAccountDto> CreateUserAsync(string role)
    {
        var email = $"reception-{Guid.NewGuid():N}@archicore.test";
        using var response = await Client.PostAsJsonAsync("/api/auth/users", new
        {
            email,
            displayName = $"Reception {role}",
            password = StrongPassword,
            role
        }, Ct);
        await AssertStatusAsync(HttpStatusCode.Created, response);
        return await ReadBodyAsync<UserAccountDto>(response);
    }

    private async Task<HttpClient> LoginAsync(string email)
    {
        var client = Fixture.CreateAnonymousClient();
        using var response = await client.PostAsJsonAsync("/api/auth/login",
            new { email, password = StrongPassword }, Ct);
        await AssertStatusAsync(HttpStatusCode.OK, response);
        var session = await ReadBodyAsync<AuthenticationResponse>(response);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);
        return client;
    }
}
