using ArchiCore.Application.Audit;
using ArchiCore.Application.Contracts;
using ArchiCore.Application.Customers;
using ArchiCore.Application.Projects;
using ArchiCore.Application.Warehouses;
using Microsoft.Extensions.DependencyInjection;

namespace ArchiCore.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<CustomerService>();
        services.AddScoped<ContractService>();
        services.AddScoped<ProjectService>();
        services.AddScoped<WarehouseService>();
        services.AddScoped<DomainEventAuditService>();
        return services;
    }
}
