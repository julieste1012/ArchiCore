using ArchiCore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ArchiCore.Domain.Tests.Persistence;

public sealed class PersistenceModelTests
{
    [Fact]
    public void PostgreSql_model_builds_and_generates_core_schema()
    {
        var options = new DbContextOptionsBuilder<ArchiCoreDbContext>()
            .UseNpgsql("Host=localhost;Database=archicore_model_test;Username=archicore;Password=unused")
            .Options;

        using var context = new ArchiCoreDbContext(options);
        var script = context.Database.GenerateCreateScript();

        Assert.Contains("CREATE TABLE \"Customers\"", script, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("CREATE TABLE \"Contracts\"", script, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("CREATE TABLE \"Projects\"", script, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("CREATE TABLE \"Warehouses\"", script, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("CustomerContracts", script, StringComparison.Ordinal);
        Assert.Contains("ContractProjects", script, StringComparison.Ordinal);
        Assert.Contains("ProjectWarehouseAssignments", script, StringComparison.Ordinal);
    }
}
