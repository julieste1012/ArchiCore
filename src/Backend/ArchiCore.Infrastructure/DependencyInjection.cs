using ArchiCore.Application.Abstractions.Persistence;
using ArchiCore.Infrastructure.Persistence;
using ArchiCore.Infrastructure.Persistence.Repositories;
using ArchiCore.Infrastructure.Persistence.Outbox;
using ArchiCore.Application.Authentication;
using ArchiCore.Infrastructure.Security;
using ArchiCore.Application.Abstractions.Storage;
using ArchiCore.Infrastructure.Storage;
using Minio;
using Microsoft.AspNetCore.Identity;
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

        var storageSettings = ObjectStorageSettings.FromConfiguration(configuration);
        services.AddSingleton(storageSettings);
        if (storageSettings.Provider.Equals("Local", StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton<IObjectStorage>(provider =>
            {
                var environment = provider.GetRequiredService<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
                var root = Path.IsPathRooted(storageSettings.LocalPath)
                    ? storageSettings.LocalPath
                    : Path.Combine(environment.ContentRootPath, storageSettings.LocalPath);
                return new LocalFileSystemObjectStorage(root, storageSettings.MaxObjectSizeBytes);
            });
        }
        else
        {
            services.AddSingleton<IMinioClient>(_ => new MinioClient()
                .WithEndpoint(storageSettings.Endpoint!)
                .WithCredentials(storageSettings.AccessKey!, storageSettings.SecretKey!)
                .WithSSL(storageSettings.Secure)
                .Build());
            services.AddSingleton<IObjectStorage, MinioObjectStorage>();
        }
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<ArchiCoreDbContext>());
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IContractRepository, ContractRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IWarehouseRepository, WarehouseRepository>();
        services.AddScoped<IDomainEventAuditRepository, DomainEventAuditRepository>();
        services.AddSingleton<IPasswordHasher<UserAccount>, PasswordHasher<UserAccount>>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<BootstrapAdminSeeder>();
        services.AddScoped<IDomainEventOutboxProcessor, DomainEventOutboxProcessor>();
        return services;
    }
}
