namespace ArchiCore.Application.Authentication;

/// <summary>
/// Approved business-role names from docs/02_business/02_BusinessRoles.md.
/// Permission assignments remain conservative until their matrix is approved.
/// </summary>
public static class AppRoles
{
    public const string SystemAdministrator = "SystemAdministrator";
    public const string AdministrativeManager = "AdministrativeManager";
    public const string ProjectCoordinator = "ProjectCoordinator";
    public const string ArchiveTechnician = "ArchiveTechnician";
    public const string ArchiveAssistant = "ArchiveAssistant";
    public const string DigitizationOperator = "DigitizationOperator";
    public const string BillingAssistant = "BillingAssistant";
    public const string CustomerRepresentative = "CustomerRepresentative";

    public static IReadOnlySet<string> All { get; } = new HashSet<string>(StringComparer.Ordinal)
    {
        SystemAdministrator,
        AdministrativeManager,
        ProjectCoordinator,
        ArchiveTechnician,
        ArchiveAssistant,
        DigitizationOperator,
        BillingAssistant,
        CustomerRepresentative
    };
}
