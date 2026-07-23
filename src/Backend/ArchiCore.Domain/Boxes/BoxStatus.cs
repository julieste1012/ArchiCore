namespace ArchiCore.Domain.Boxes;

/// <summary>
/// Estados posibles del ciclo de vida de una <see cref="Box"/>.
/// </summary>
public enum BoxStatus
{
    Created,
    Available,
    InStorage,
    InTransit,
    LoanProcess,
    Returned,
    Archived
}
