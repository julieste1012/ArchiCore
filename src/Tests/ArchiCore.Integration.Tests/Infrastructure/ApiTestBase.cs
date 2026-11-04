using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace ArchiCore.Integration.Tests.Infrastructure;

/// <summary>
/// Base class for the HTTP tests. Each test creates the data it needs; test data is unique
/// and assertions identify records by ID rather than relying on list positions.
/// </summary>
public abstract class ApiTestBase : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    protected ApiTestBase(ApiFixture fixture)
    {
        Fixture = fixture;
        Client = fixture.CreateClient();
    }

    protected ApiFixture Fixture { get; }

    protected HttpClient Client { get; }

    protected static CancellationToken Ct => TestContext.Current.CancellationToken;

    public void Dispose()
    {
        Client.Dispose();
        GC.SuppressFinalize(this);
    }

    protected Task<HttpResponseMessage> GetAsync(string uri)
        => Client.GetAsync(uri, Ct);

    protected Task<HttpResponseMessage> PostAsync(string uri, object? body)
        => Client.PostAsJsonAsync(uri, body, JsonOptions, Ct);

    protected static async Task AssertStatusAsync(HttpStatusCode expected, HttpResponseMessage response)
    {
        if (response.StatusCode == expected)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync(Ct);
        Assert.Fail(
            $"Expected HTTP {(int)expected} ({expected}) from {response.RequestMessage?.Method} " +
            $"{response.RequestMessage?.RequestUri?.AbsolutePath}, but received {(int)response.StatusCode} " +
            $"({response.StatusCode}). Body: {body}");
    }

    protected static async Task<T> ReadBodyAsync<T>(HttpResponseMessage response)
    {
        var body = await response.Content.ReadFromJsonAsync<T>(JsonOptions, Ct);
        return body ?? throw new InvalidOperationException(
            $"The response body could not be read as {typeof(T).Name}.");
    }

    protected async Task<T> GetJsonAsync<T>(string uri)
    {
        using var response = await GetAsync(uri);
        await AssertStatusAsync(HttpStatusCode.OK, response);
        return await ReadBodyAsync<T>(response);
    }

    /// <summary>
    /// Searches a paged endpoint page by page (100 items per page) until the predicate matches.
    /// Never assumes the list is empty or that the item occupies a fixed position.
    /// </summary>
    protected async Task<T?> FindInListAsync<T>(string path, Func<T, bool> predicate) where T : class
    {
        for (var page = 1; ; page++)
        {
            var separator = path.Contains('?') ? '&' : '?';
            var paged = await GetJsonAsync<PagedResponse<T>>($"{path}{separator}page={page}&pageSize=100");

            var match = paged.Items.FirstOrDefault(predicate);
            if (match is not null)
            {
                return match;
            }

            if (page >= paged.TotalPages)
            {
                return null;
            }
        }
    }

    protected async Task<CustomerResponse> CreateCustomerAsync(string? nit = null)
    {
        using var response = await PostAsync("/api/customers", new { Nit = nit ?? TestData.Nit() });
        await AssertStatusAsync(HttpStatusCode.Created, response);
        return await ReadBodyAsync<CustomerResponse>(response);
    }

    protected async Task<WarehouseResponse> CreateWarehouseAsync(string? code = null)
    {
        using var response = await PostAsync(
            "/api/warehouses",
            new { WarehouseCode = code ?? TestData.WarehouseCode() });
        await AssertStatusAsync(HttpStatusCode.Created, response);
        return await ReadBodyAsync<WarehouseResponse>(response);
    }

    protected async Task<ContractResponse> CreateContractAsync(
        Guid clientId,
        string? contractNumber = null,
        DateTimeOffset? startDate = null,
        decimal totalValue = 1_500_000.75m,
        string currency = "COP")
    {
        var request = new
        {
            ContractNumber = contractNumber ?? TestData.ContractNumber(),
            ClientId = clientId,
            StartDate = startDate ?? FixedDate,
            TotalValue = totalValue,
            Currency = currency
        };

        using var response = await PostAsync("/api/contracts", request);
        await AssertStatusAsync(HttpStatusCode.Created, response);
        return await ReadBodyAsync<ContractResponse>(response);
    }

    protected async Task<ProjectResponse> CreateProjectAsync(Guid contractId, string? projectCode = null)
    {
        using var response = await PostAsync(
            "/api/projects",
            new { ProjectCode = projectCode ?? TestData.ProjectCode(), ContractId = contractId });
        await AssertStatusAsync(HttpStatusCode.Created, response);
        return await ReadBodyAsync<ProjectResponse>(response);
    }

    protected static DateTimeOffset FixedDate { get; } = new(2026, 1, 15, 8, 30, 0, TimeSpan.Zero);
}
