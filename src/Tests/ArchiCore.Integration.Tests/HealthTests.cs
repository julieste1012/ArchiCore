using System.Net;
using ArchiCore.Integration.Tests.Infrastructure;
using Xunit;

namespace ArchiCore.Integration.Tests;

[Collection(IntegrationCollection.Name)]
public sealed class HealthTests(ApiFixture fixture) : ApiTestBase(fixture)
{
    [Fact]
    public async Task Live_returns_200_and_healthy()
    {
        using var response = await GetAsync("/health/live");

        await AssertStatusAsync(HttpStatusCode.OK, response);
        var body = await ReadBodyAsync<HealthResponse>(response);
        Assert.Equal("Healthy", body.Status);
    }

    [Fact]
    public async Task Ready_returns_200_with_database_connected()
    {
        using var response = await GetAsync("/health/ready");

        await AssertStatusAsync(HttpStatusCode.OK, response);
        var body = await ReadBodyAsync<HealthResponse>(response);
        Assert.Equal("Ready", body.Status);
        Assert.Equal("Connected", body.Database);
    }
}
