using ArchiCore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Xunit;

namespace ArchiCore.Integration.Tests.Infrastructure;

/// <summary>
/// Collection fixture: one throw-away PostgreSQL 17 container and one in-memory API host
/// shared by the integration suite. Never touches the developer's Docker Compose database.
/// </summary>
public sealed class ApiFixture : IAsyncLifetime
{
    public const string DatabaseName = "archicore_integration_tests";

    private const string ConnectionStringVariable = "ConnectionStrings__ArchiCoreDb";

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17-alpine")
        .WithDatabase(DatabaseName)
        .WithUsername("archicore_tests")
        .WithPassword(Guid.NewGuid().ToString("N"))
        .Build();

    private string? _previousConnectionString;
    private ArchiCoreApiFactory? _factory;

    public string ConnectionString => _postgres.GetConnectionString();

    public ArchiCoreApiFactory Factory
        => _factory ?? throw new InvalidOperationException("The fixture has not been initialized.");

    public async ValueTask InitializeAsync()
    {
        await _postgres.StartAsync();

        // Program.cs reads the connection string while configuring dependency injection.
        // Preserve and restore any developer-level value after the fixture is disposed.
        _previousConnectionString = Environment.GetEnvironmentVariable(ConnectionStringVariable);
        Environment.SetEnvironmentVariable(ConnectionStringVariable, ConnectionString);

        try
        {
            _factory = new ArchiCoreApiFactory(ConnectionString);

            // Force host creation, then apply the repository's real migrations to the temporary DB.
            await using var scope = _factory.Services.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ArchiCoreDbContext>();
            await dbContext.Database.MigrateAsync();
        }
        catch
        {
            if (_factory is not null)
            {
                await _factory.DisposeAsync();
                _factory = null;
            }

            await _postgres.DisposeAsync();
            Environment.SetEnvironmentVariable(ConnectionStringVariable, _previousConnectionString);
            throw;
        }
    }

    public HttpClient CreateClient()
        => Factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    public async Task<T> WithDbContextAsync<T>(Func<ArchiCoreDbContext, Task<T>> action)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ArchiCoreDbContext>();
        return await action(dbContext);
    }

    public async ValueTask DisposeAsync()
    {
        if (_factory is not null)
        {
            await _factory.DisposeAsync();
        }

        await _postgres.DisposeAsync();
        Environment.SetEnvironmentVariable(ConnectionStringVariable, _previousConnectionString);
    }
}
