namespace ArchiCore.Domain.Projects;

/// <summary>
/// Estados posibles del ciclo de vida de un <see cref="Project"/>.
/// </summary>
public enum ProjectStatus
{
    Draft,
    Planned,
    Active,
    Suspended,
    Completed,
    Closed,
    Archived
}
