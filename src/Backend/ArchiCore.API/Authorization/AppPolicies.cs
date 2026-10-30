using ArchiCore.Application.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace ArchiCore.API.Authorization;

/// <summary>
/// Authorization rules derived from the approved business-role responsibilities
/// and responsibility matrix in docs/02_business/02_BusinessRoles.md.
/// Roles receive only the operations required by their documented duties.
/// </summary>
public static class AppPolicies
{
    public const string CustomersRead = "Customers.Read";
    public const string CustomersManage = "Customers.Manage";
    public const string ContractsRead = "Contracts.Read";
    public const string ContractsManage = "Contracts.Manage";
    public const string ProjectsRead = "Projects.Read";
    public const string ProjectsOperate = "Projects.Operate";
    public const string ProjectsAssignWarehouses = "Projects.AssignWarehouses";
    public const string WarehousesRead = "Warehouses.Read";
    public const string WarehousesManage = "Warehouses.Manage";

    public static IServiceCollection AddArchiCoreAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(CustomersRead, policy => policy.RequireRole(
                AppRoles.SystemAdministrator,
                AppRoles.AdministrativeManager));

            options.AddPolicy(CustomersManage, policy => policy.RequireRole(
                AppRoles.SystemAdministrator,
                AppRoles.AdministrativeManager));

            options.AddPolicy(ContractsRead, policy => policy.RequireRole(
                AppRoles.SystemAdministrator,
                AppRoles.AdministrativeManager,
                AppRoles.ProjectCoordinator,
                AppRoles.ArchiveTechnician,
                AppRoles.ArchiveAssistant,
                AppRoles.DigitizationOperator,
                AppRoles.BillingAssistant));

            options.AddPolicy(ContractsManage, policy => policy.RequireRole(
                AppRoles.SystemAdministrator,
                AppRoles.AdministrativeManager));

            options.AddPolicy(ProjectsRead, policy => policy.RequireRole(
                AppRoles.SystemAdministrator,
                AppRoles.AdministrativeManager,
                AppRoles.ProjectCoordinator,
                AppRoles.BillingAssistant));

            options.AddPolicy(ProjectsOperate, policy => policy.RequireRole(
                AppRoles.SystemAdministrator,
                AppRoles.ProjectCoordinator));

            options.AddPolicy(ProjectsAssignWarehouses, policy => policy.RequireRole(
                AppRoles.SystemAdministrator,
                AppRoles.ProjectCoordinator));

            options.AddPolicy(WarehousesRead, policy => policy.RequireRole(
                AppRoles.SystemAdministrator,
                AppRoles.ProjectCoordinator,
                AppRoles.ArchiveTechnician,
                AppRoles.ArchiveAssistant));

            // Warehouse creation and lifecycle administration remain technical/
            // infrastructure administration. Physical location operations will get
            // dedicated endpoints for the ArchiveAssistant role in the storage module.
            options.AddPolicy(WarehousesManage, policy => policy.RequireRole(
                AppRoles.SystemAdministrator));
        });

        return services;
    }
}
