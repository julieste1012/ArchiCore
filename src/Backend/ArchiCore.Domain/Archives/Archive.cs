using ArchiCore.Domain.Archives.Events;
using ArchiCore.Domain.Common;

namespace ArchiCore.Domain.Archives;

/// <summary>
/// Aggregate Root que mantiene la clasificación archivística de un proyecto.
/// Las carpetas y cajas son agregados independientes que referencian las identidades de los expedientes.
/// </summary>
public sealed class Archive : Entity, IAggregateRoot
{
    private readonly List<ArchiveSeries> _series = new();

    public Guid ProjectId { get; private set; }
    public IReadOnlyCollection<ArchiveSeries> Series => _series.AsReadOnly();

    private Archive() { }
    private Archive(Guid projectId) => ProjectId = projectId;

    public static Result<Archive> Create(Guid projectId)
    {
        if (projectId == Guid.Empty)
            return Result.Failure<Archive>(new Error("Archive.ProjectRequired", "El archivo documental debe pertenecer a un proyecto."));

        var archive = new Archive(projectId);
        archive.AddDomainEvent(new ArchiveCreated(archive.Id, projectId));
        return Result.Success(archive);
    }

    public Result<Guid> AddSeries(string code, string name)
    {
        var validation = ValidateCodeAndName(code, name);
        if (validation.IsFailure)
            return Result.Failure<Guid>(validation.Error);

        var normalizedCode = code.Trim().ToUpperInvariant();
        if (_series.Any(item => string.Equals(item.Code, normalizedCode, StringComparison.OrdinalIgnoreCase)))
            return Result.Failure<Guid>(new Error("Archive.DuplicateSeriesCode", "El código de serie ya existe en este archivo."));

        var series = new ArchiveSeries(normalizedCode, name.Trim());
        _series.Add(series);
        MarkAsUpdated();
        AddDomainEvent(new SeriesCreated(Id, series.Id, series.Code, series.Name));
        return Result.Success(series.Id);
    }

    public Result<Guid> AddSubseries(Guid seriesId, string code, string name)
    {
        if (seriesId == Guid.Empty)
            return Result.Failure<Guid>(new Error("Archive.SeriesRequired", "La serie documental es obligatoria."));

        var validation = ValidateCodeAndName(code, name);
        if (validation.IsFailure)
            return Result.Failure<Guid>(validation.Error);

        var series = _series.FirstOrDefault(item => item.Id == seriesId);
        if (series is null)
            return Result.Failure<Guid>(new Error("Archive.SeriesNotFound", "La serie no pertenece a este archivo."));

        var normalizedCode = code.Trim().ToUpperInvariant();
        var result = series.AddSubseries(normalizedCode, name.Trim());
        if (result.IsFailure)
            return result;

        MarkAsUpdated();
        AddDomainEvent(new SubseriesCreated(Id, seriesId, result.Value, normalizedCode, name.Trim()));
        return result;
    }

    public Result<Guid> AddExpedient(Guid subseriesId, string reference, string title)
    {
        if (subseriesId == Guid.Empty)
            return Result.Failure<Guid>(new Error("Archive.SubseriesRequired", "La subserie documental es obligatoria."));
        if (string.IsNullOrWhiteSpace(reference) || reference.Trim().Length > 100)
            return Result.Failure<Guid>(new Error("Archive.InvalidExpedientReference", "La referencia del expediente es obligatoria y no puede superar 100 caracteres."));
        if (string.IsNullOrWhiteSpace(title) || title.Trim().Length > 300)
            return Result.Failure<Guid>(new Error("Archive.InvalidExpedientTitle", "El título del expediente es obligatorio y no puede superar 300 caracteres."));

        var subseries = _series.SelectMany(item => item.Subseries).FirstOrDefault(item => item.Id == subseriesId);
        if (subseries is null)
            return Result.Failure<Guid>(new Error("Archive.SubseriesNotFound", "La subserie no pertenece a este archivo."));

        var normalizedReference = reference.Trim();
        var result = subseries.AddExpedient(normalizedReference, title.Trim());
        if (result.IsFailure)
            return result;

        MarkAsUpdated();
        AddDomainEvent(new ExpedientCreated(Id, subseriesId, result.Value, normalizedReference, title.Trim()));
        return result;
    }

    public bool ContainsExpedient(Guid expedientId)
        => expedientId != Guid.Empty && _series.SelectMany(item => item.Subseries).Any(item => item.ContainsExpedient(expedientId));

    private static Result ValidateCodeAndName(string code, string name)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Trim().Length > 50)
            return Result.Failure(new Error("Archive.InvalidClassificationCode", "El código de clasificación es obligatorio y no puede superar 50 caracteres."));
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 250)
            return Result.Failure(new Error("Archive.InvalidClassificationName", "El nombre de clasificación es obligatorio y no puede superar 250 caracteres."));
        return Result.Success();
    }
}
