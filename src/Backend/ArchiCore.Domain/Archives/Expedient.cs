using ArchiCore.Domain.Common;

namespace ArchiCore.Domain.Archives;

/// <summary>Expediente mínimo. Reference no codifica una regla oficial de numeración AGN.</summary>
public sealed class Expedient : Entity
{
    public string Reference { get; private set; }
    public string Title { get; private set; }

    private Expedient()
    {
        Reference = string.Empty;
        Title = string.Empty;
    }

    internal Expedient(string reference, string title)
    {
        Reference = reference;
        Title = title;
    }
}
