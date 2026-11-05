using ArchiCore.Application.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace ArchiCore.API.Authorization;

/// <summary>
/// Authorization rules derived from the approved business-role responsibilities
/// and responsibility matrix in docs/02_business/02_BusinessRoles.md.
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
    public const string ReceptionsRead = "Receptions.Read";
    public const string ReceptionsReceive = "Receptions.Receive";
    public const string ReceptionsInspect = "Receptions.Inspect";
    public const string ReceptionsCount = "Receptions.Count";
    public const string ReceptionsApprove = "Receptions.Approve";

    public const string ArchivesRead = "Archives.Read";
    public const string ArchivesCreate = "Archives.Create";
    public const string ArchivesManage = "Archives.Manage";
    public const string BoxesRead = "Boxes.Read";
    public const string BoxesCreate = "Boxes.Create";
    public const string BoxesPhysical = "Boxes.Physical";
    public const string FoldersRead = "Folders.Read";
    public const string FoldersCreate = "Folders.Create";
    public const string FoldersReceive = "Folders.Receive";
    public const string FoldersManage = "Folders.Manage";
    public const string FoldersAssign = "Folders.Assign";
    public const string FoldersPhysical = "Folders.Physical";
    public const string LoansRead = "Loans.Read";
    public const string LoansRequest = "Loans.Request";
    public const string LoansAuthorize = "Loans.Authorize";
    public const string LoansDeliver = "Loans.Deliver";
    public const string DigitizationRead = "Digitization.Read";
    public const string DigitizationRequest = "Digitization.Request";
    public const string DigitizationOperate = "Digitization.Operate";

    public static IServiceCollection AddArchiCoreAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(CustomersRead, policy => policy.RequireRole(
                AppRoles.SystemAdministrator, AppRoles.AdministrativeManager));
            options.AddPolicy(CustomersManage, policy => policy.RequireRole(
                AppRoles.SystemAdministrator, AppRoles.AdministrativeManager));
            options.AddPolicy(ContractsRead, policy => policy.RequireRole(
                AppRoles.SystemAdministrator, AppRoles.AdministrativeManager,
                AppRoles.ProjectCoordinator, AppRoles.BillingAssistant));
            options.AddPolicy(ContractsManage, policy => policy.RequireRole(
                AppRoles.SystemAdministrator, AppRoles.AdministrativeManager));
            options.AddPolicy(ProjectsRead, policy => policy.RequireRole(
                AppRoles.SystemAdministrator, AppRoles.AdministrativeManager,
                AppRoles.ProjectCoordinator, AppRoles.ArchiveTechnician,
                AppRoles.ArchiveAssistant, AppRoles.DigitizationOperator, AppRoles.BillingAssistant));
            options.AddPolicy(ProjectsOperate, policy => policy.RequireRole(
                AppRoles.SystemAdministrator, AppRoles.ProjectCoordinator));
            options.AddPolicy(ProjectsAssignWarehouses, policy => policy.RequireRole(
                AppRoles.SystemAdministrator, AppRoles.ProjectCoordinator));
            options.AddPolicy(WarehousesRead, policy => policy.RequireRole(
                AppRoles.SystemAdministrator, AppRoles.ProjectCoordinator,
                AppRoles.ArchiveTechnician, AppRoles.ArchiveAssistant));
            options.AddPolicy(WarehousesManage, policy => policy.RequireRole(AppRoles.SystemAdministrator));

            options.AddPolicy(ReceptionsRead, policy => policy.RequireRole(
                AppRoles.SystemAdministrator, AppRoles.ProjectCoordinator,
                AppRoles.ArchiveTechnician, AppRoles.ArchiveAssistant));
            options.AddPolicy(ReceptionsReceive, policy => policy.RequireRole(
                AppRoles.SystemAdministrator, AppRoles.ProjectCoordinator));
            options.AddPolicy(ReceptionsInspect, policy => policy.RequireRole(
                AppRoles.SystemAdministrator, AppRoles.ProjectCoordinator, AppRoles.ArchiveAssistant));
            options.AddPolicy(ReceptionsCount, policy => policy.RequireRole(
                AppRoles.SystemAdministrator, AppRoles.ProjectCoordinator, AppRoles.ArchiveAssistant));
            options.AddPolicy(ReceptionsApprove, policy => policy.RequireRole(
                AppRoles.SystemAdministrator, AppRoles.ProjectCoordinator));

            options.AddPolicy(ArchivesRead, policy => policy.RequireRole(
                AppRoles.SystemAdministrator, AppRoles.ProjectCoordinator,
                AppRoles.ArchiveTechnician, AppRoles.ArchiveAssistant, AppRoles.DigitizationOperator));
            options.AddPolicy(ArchivesCreate, policy => policy.RequireRole(
                AppRoles.SystemAdministrator, AppRoles.ProjectCoordinator));
            options.AddPolicy(ArchivesManage, policy => policy.RequireRole(
                AppRoles.SystemAdministrator, AppRoles.ArchiveTechnician));

            options.AddPolicy(BoxesRead, policy => policy.RequireRole(
                AppRoles.SystemAdministrator, AppRoles.ProjectCoordinator,
                AppRoles.ArchiveTechnician, AppRoles.ArchiveAssistant));
            options.AddPolicy(BoxesCreate, policy => policy.RequireRole(
                AppRoles.SystemAdministrator, AppRoles.ArchiveTechnician));
            options.AddPolicy(BoxesPhysical, policy => policy.RequireRole(
                AppRoles.SystemAdministrator, AppRoles.ArchiveAssistant));

            options.AddPolicy(FoldersRead, policy => policy.RequireRole(
                AppRoles.SystemAdministrator, AppRoles.ProjectCoordinator,
                AppRoles.ArchiveTechnician, AppRoles.ArchiveAssistant, AppRoles.DigitizationOperator));
            options.AddPolicy(FoldersCreate, policy => policy.RequireRole(
                AppRoles.SystemAdministrator, AppRoles.ProjectCoordinator, AppRoles.ArchiveTechnician));
            options.AddPolicy(FoldersReceive, policy => policy.RequireRole(
                AppRoles.SystemAdministrator, AppRoles.ProjectCoordinator, AppRoles.ArchiveAssistant));
            options.AddPolicy(FoldersManage, policy => policy.RequireRole(
                AppRoles.SystemAdministrator, AppRoles.ArchiveTechnician));
            options.AddPolicy(FoldersAssign, policy => policy.RequireRole(
                AppRoles.SystemAdministrator, AppRoles.ProjectCoordinator));
            options.AddPolicy(FoldersPhysical, policy => policy.RequireRole(
                AppRoles.SystemAdministrator, AppRoles.ArchiveAssistant));

            options.AddPolicy(LoansRead, policy => policy.RequireRole(
                AppRoles.SystemAdministrator, AppRoles.ProjectCoordinator, AppRoles.ArchiveAssistant));
            options.AddPolicy(LoansRequest, policy => policy.RequireRole(
                AppRoles.SystemAdministrator, AppRoles.ProjectCoordinator));
            options.AddPolicy(LoansAuthorize, policy => policy.RequireRole(
                AppRoles.SystemAdministrator, AppRoles.ProjectCoordinator));
            options.AddPolicy(LoansDeliver, policy => policy.RequireRole(
                AppRoles.SystemAdministrator, AppRoles.ArchiveAssistant));

            options.AddPolicy(DigitizationRead, policy => policy.RequireRole(
                AppRoles.SystemAdministrator, AppRoles.ProjectCoordinator,
                AppRoles.ArchiveTechnician, AppRoles.DigitizationOperator));
            options.AddPolicy(DigitizationRequest, policy => policy.RequireRole(
                AppRoles.SystemAdministrator, AppRoles.ProjectCoordinator, AppRoles.ArchiveTechnician));
            options.AddPolicy(DigitizationOperate, policy => policy.RequireRole(
                AppRoles.SystemAdministrator, AppRoles.DigitizationOperator));
        });

        return services;
    }
}
