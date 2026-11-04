using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ArchiCore.Infrastructure.Persistence;

public sealed class ArchiCoreDbContextFactory : IDesignTimeDbContextFactory<ArchiCoreDbContext>
{
    public ArchiCoreDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__ArchiCoreDb")
            ?? "Host=127.0.0.1;Port=5433;Database=archicore;Username=archicore;Password=archicore_dev";

        var options = new DbContextOptionsBuilder<ArchiCoreDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new ArchiCoreDbContext(options);
    }
}
