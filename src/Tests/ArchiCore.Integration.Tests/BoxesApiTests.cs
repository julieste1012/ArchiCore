using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ArchiCore.Application.Authentication;
using ArchiCore.Integration.Tests.Infrastructure;
using Xunit;

namespace ArchiCore.Integration.Tests;

[Collection(IntegrationCollection.Name)]
public sealed class BoxesApiTests(ApiFixture fixture) : ApiTestBase(fixture)
{
    private const string StrongPassword = "Box_Test_2026!Strong";

    [Fact]
    public async Task Create_get_and_list_box_by_status()
    {
        var code = NewBoxCode();
        using var createdResponse = await PostAsync("/api/boxes", new { boxCode = code });
        await AssertStatusAsync(HttpStatusCode.Created, createdResponse);

        var created = await ReadBodyAsync<BoxResponse>(createdResponse);
        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.Equal(code, created.BoxCode);
        Assert.Null(created.Barcode);
        Assert.Equal("Created", created.Status);
        Assert.EndsWith($"/api/boxes/{created.Id}", createdResponse.Headers.Location?.AbsolutePath);

        var fetched = await GetJsonAsync<BoxResponse>($"/api/boxes/{created.Id}");
        Assert.Equal(created.Id, fetched.Id);
        Assert.Equal(code, fetched.BoxCode);

        var listed = await GetJsonAsync<PagedResponse<BoxResponse>>(
            "/api/boxes?status=Created&page=1&pageSize=100");
        Assert.Contains(listed.Items, box => box.Id == created.Id);
    }

    [Fact]
    public async Task Duplicate_box_code_returns_conflict()
    {
        var code = NewBoxCode();
        using var first = await PostAsync("/api/boxes", new { boxCode = code });
        await AssertStatusAsync(HttpStatusCode.Created, first);

        using var duplicate = await PostAsync("/api/boxes", new { boxCode = code });
        await AssertStatusAsync(HttpStatusCode.Conflict, duplicate);
        var problem = await ReadBodyAsync<ProblemResponse>(duplicate);
        Assert.Equal("Box.DuplicateCode", problem.Code);
    }

    [Fact]
    public async Task Barcode_is_unique_and_cannot_be_reassigned_to_a_different_value()
    {
        var first = await CreateBoxAsync();
        var second = await CreateBoxAsync();
        var barcode = NewBarcode();

        using var assigned = await PostAsync(
            $"/api/boxes/{first.Id}/barcode", new { barcode });
        await AssertStatusAsync(HttpStatusCode.OK, assigned);
        var updated = await ReadBodyAsync<BoxResponse>(assigned);
        Assert.Equal(barcode, updated.Barcode);

        using var reassigned = await PostAsync(
            $"/api/boxes/{first.Id}/barcode", new { barcode = NewBarcode() });
        await AssertStatusAsync(HttpStatusCode.Conflict, reassigned);
        var reassignmentProblem = await ReadBodyAsync<ProblemResponse>(reassigned);
        Assert.Equal("Box.BarcodeAlreadyAssigned", reassignmentProblem.Code);

        using var duplicate = await PostAsync(
            $"/api/boxes/{second.Id}/barcode", new { barcode });
        await AssertStatusAsync(HttpStatusCode.Conflict, duplicate);
        var duplicateProblem = await ReadBodyAsync<ProblemResponse>(duplicate);
        Assert.Equal("Box.DuplicateBarcode", duplicateProblem.Code);
    }

    [Fact]
    public async Task Archive_technician_can_register_and_label_boxes_but_archive_assistant_is_read_only()
    {
        using var technician = await LoginAsRoleAsync(AppRoles.ArchiveTechnician);
        var code = NewBoxCode();

        using var created = await technician.PostAsJsonAsync(
            "/api/boxes", new { boxCode = code }, Ct);
        await AssertStatusAsync(HttpStatusCode.Created, created);
        var box = await ReadBodyAsync<BoxResponse>(created);

        using var assigned = await technician.PostAsJsonAsync(
            $"/api/boxes/{box.Id}/barcode", new { barcode = NewBarcode() }, Ct);
        await AssertStatusAsync(HttpStatusCode.OK, assigned);

        using var assistant = await LoginAsRoleAsync(AppRoles.ArchiveAssistant);
        using var list = await assistant.GetAsync("/api/boxes", Ct);
        await AssertStatusAsync(HttpStatusCode.OK, list);

        using var forbidden = await assistant.PostAsJsonAsync(
            "/api/boxes", new { boxCode = NewBoxCode() }, Ct);
        await AssertStatusAsync(HttpStatusCode.Forbidden, forbidden);
    }

    private async Task<BoxResponse> CreateBoxAsync()
    {
        using var response = await PostAsync("/api/boxes", new { boxCode = NewBoxCode() });
        await AssertStatusAsync(HttpStatusCode.Created, response);
        return await ReadBodyAsync<BoxResponse>(response);
    }

    private async Task<HttpClient> LoginAsRoleAsync(string role)
    {
        var email = $"box-{Guid.NewGuid():N}@archicore.test";
        using var created = await Client.PostAsJsonAsync("/api/auth/users", new
        {
            email,
            displayName = $"Box {role}",
            password = StrongPassword,
            role
        }, Ct);
        await AssertStatusAsync(HttpStatusCode.Created, created);

        var client = Fixture.CreateAnonymousClient();
        using var login = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email,
            password = StrongPassword
        }, Ct);
        await AssertStatusAsync(HttpStatusCode.OK, login);
        var session = await ReadBodyAsync<AuthenticationResponse>(login);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", session.AccessToken);
        return client;
    }

    private static string NewBoxCode()
        => $"BOX-{Guid.NewGuid():N}"[..12].ToUpperInvariant();

    private static string NewBarcode()
        => $"BC-{Guid.NewGuid():N}"[..15].ToUpperInvariant();
}
