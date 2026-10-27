using ArchiCore.Application.Abstractions.Persistence;
using ArchiCore.Infrastructure.Persistence;
using ArchiCore.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ArchiCore.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ArchiCoreDb");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Missing connection string 'ConnectionStrings:ArchiCoreDb'. Configure it through appsettings.Development.json or environment variable ConnectionStrings__ArchiCoreDb.");
        }

        services.AddDbContext<ArchiCoreDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<ArchiCoreDbContext>());
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IContractRepository, ContractRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IWarehouseRepository, WarehouseRepository>();
        return services;
    }
}
