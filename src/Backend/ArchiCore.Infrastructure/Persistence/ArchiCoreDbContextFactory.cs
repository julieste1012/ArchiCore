using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ArchiCore.Infrastructure.Persistence;

public sealed class ArchiCoreDbContextFactory : IDesignTimeDbContextFactory<ArchiCoreDbContext>
{
    public ArchiCoreDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__ArchiCoreDb")
            ?? "Host=localhost;Port=5432;Database=archicore;Username=archicore;Password=archicore_dev";

        var options = new DbContextOptionsBuilder<ArchiCoreDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new ArchiCoreDbContext(options);
    }
}
