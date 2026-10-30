using ArchiCore.Application.Audit;
using ArchiCore.Application.Receptions;
using ArchiCore.Application.Boxes;
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
        services.AddScoped<SecurityAuditService>();
        services.AddScoped<DocumentaryReceptionService>();
        services.AddScoped<BoxService>();
        return services;
    }
}
