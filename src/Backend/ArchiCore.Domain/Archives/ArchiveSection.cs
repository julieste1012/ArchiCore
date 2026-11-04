using ArchiCore.Domain.Common;

namespace ArchiCore.Domain.Archives;

/// <summary>Sección documental interna del Fondo (Archive).</summary>
public sealed class ArchiveSection : Entity
{
    private readonly List<ArchiveSeries> _series = new();

    public string Code { get; private set; }
    public string Name { get; private set; }
    public IReadOnlyCollection<ArchiveSeries> Series => _series.AsReadOnly();

    private ArchiveSection()
    {
        Code = string.Empty;
        Name = string.Empty;
    }

    internal ArchiveSection(string code, string name)
    {
        Code = code;
        Name = name;
    }

    internal Result<Guid> AddSeries(string code, string name)
    {
        if (_series.Any(item => string.Equals(item.Code, code, StringComparison.OrdinalIgnoreCase)))
            return Result.Failure<Guid>(new Error("Archive.DuplicateSeriesCode", "El código de serie ya existe dentro de la sección."));

        var series = new ArchiveSeries(code, name);
        _series.Add(series);
        MarkAsUpdated();
        return Result.Success(series.Id);
    }
}
