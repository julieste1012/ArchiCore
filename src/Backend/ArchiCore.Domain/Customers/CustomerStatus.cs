namespace ArchiCore.Domain.Customers;

/// <summary>
/// Estados posibles del ciclo de vida de un <see cref="Customer"/>.
/// </summary>
public enum CustomerStatus
{
    Prospect,
    Active,
    Inactive,
    Archived
}
