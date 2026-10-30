namespace ArchiCore.Application.Authentication;

/// <summary>
/// Official business-role identifiers defined in docs/02_business/02_BusinessRoles.md.
/// The API authorization policies map these roles to operations using that document's
/// approved responsibilities and responsibility matrix.
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
