using ArchiCore.Domain.Common;

namespace ArchiCore.Domain.Archives;

public sealed class ArchiveSeries : Entity
{
    private readonly List<ArchiveSubseries> _subseries = new();

    public string Code { get; private set; }
    public string Name { get; private set; }
    public IReadOnlyCollection<ArchiveSubseries> Subseries => _subseries.AsReadOnly();

    internal ArchiveSeries(string code, string name)
    {
        Code = code;
        Name = name;
    }

    internal Result<Guid> AddSubseries(string code, string name)
    {
        if (_subseries.Any(item => string.Equals(item.Code, code, StringComparison.OrdinalIgnoreCase)))
            return Result.Failure<Guid>(new Error("Archive.DuplicateSubseriesCode", "El código de subserie ya existe dentro de la serie."));

        var subseries = new ArchiveSubseries(code, name);
        _subseries.Add(subseries);
        MarkAsUpdated();
        return Result.Success(subseries.Id);
    }

    internal bool ContainsSubseries(Guid subseriesId) => _subseries.Any(item => item.Id == subseriesId);
}
