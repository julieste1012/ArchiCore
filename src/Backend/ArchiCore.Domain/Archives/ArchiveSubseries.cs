using ArchiCore.Domain.Common;

namespace ArchiCore.Domain.Archives;

public sealed class ArchiveSubseries : Entity
{
    private readonly List<Expedient> _expedients = new();

    public string Code { get; private set; }
    public string Name { get; private set; }
    public IReadOnlyCollection<Expedient> Expedients => _expedients.AsReadOnly();

    internal ArchiveSubseries(string code, string name)
    {
        Code = code;
        Name = name;
    }

    internal Result<Guid> AddExpedient(string reference, string title)
    {
        if (_expedients.Any(item => string.Equals(item.Reference, reference, StringComparison.OrdinalIgnoreCase)))
            return Result.Failure<Guid>(new Error("Archive.DuplicateExpedientReference", "La referencia del expediente ya existe dentro de la subserie."));

        var expedient = new Expedient(reference, title);
        _expedients.Add(expedient);
        MarkAsUpdated();
        return Result.Success(expedient.Id);
    }

    internal bool ContainsExpedient(Guid expedientId) => _expedients.Any(item => item.Id == expedientId);
}
