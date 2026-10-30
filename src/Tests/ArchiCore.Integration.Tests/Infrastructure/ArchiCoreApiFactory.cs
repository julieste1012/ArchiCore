using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ArchiCore.Integration.Tests.Infrastructure;

/// <summary>
/// Hosts the real ArchiCore API in memory (TestServer, no fixed port) against the
/// PostgreSQL connection string supplied by the test container.
/// </summary>
public sealed class ArchiCoreApiFactory(string connectionString) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // The .slnx solution format is used by this repository; resolve the API content root
        // from the test assembly layout instead of relying on WebApplicationFactory's .sln discovery.
        var apiContentRoot = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "../../../../..", "Backend", "ArchiCore.API"));

        if (!Directory.Exists(apiContentRoot))
        {
            throw new DirectoryNotFoundException(
                $"Could not locate the ArchiCore API content root at '{apiContentRoot}'.");
        }

        builder.UseContentRoot(apiContentRoot);
        builder.UseEnvironment("Testing");

        // The test container connection string is explicitly set before the host is built.
        builder.UseSetting("ConnectionStrings:ArchiCoreDb", connectionString);
    }
}
