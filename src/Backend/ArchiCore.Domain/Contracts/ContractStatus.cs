namespace ArchiCore.Domain.Contracts;

/// <summary>
/// Estados posibles del ciclo de vida de un <see cref="Contract"/>.
/// </summary>
public enum ContractStatus
{
    Draft,
    Active,
    Suspended,
    Finished,
    Cancelled
}
