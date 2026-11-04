using ArchiCore.Domain.Archives.Events;
using ArchiCore.Domain.Common;

namespace ArchiCore.Domain.Archives;

/// <summary>
/// Aggregate Root que representa el Fondo documental de un Project.
/// Su jerarquía interna sigue Section -> Series -> Subseries -> Expedient.
/// Las carpetas y cajas son agregados independientes que referencian expedientes y ubicaciones por identidad.
/// </summary>
public sealed class Archive : Entity, IAggregateRoot
{
    private readonly List<ArchiveSection> _sections = new();

    public Guid ProjectId { get; private set; }
    public IReadOnlyCollection<ArchiveSection> Sections => _sections.AsReadOnly();

    private Archive() { }
    private Archive(Guid projectId) => ProjectId = projectId;

    public static Result<Archive> Create(Guid projectId)
    {
        if (projectId == Guid.Empty)
            return Result.Failure<Archive>(new Error("Archive.ProjectRequired", "El Fondo documental debe pertenecer a un proyecto."));

        var archive = new Archive(projectId);
        archive.AddDomainEvent(new ArchiveCreated(archive.Id, projectId));
        return Result.Success(archive);
    }

    public Result<Guid> AddSection(string code, string name)
    {
        var validation = ValidateCodeAndName(code, name);
        if (validation.IsFailure)
            return Result.Failure<Guid>(validation.Error);

        var normalizedCode = code.Trim().ToUpperInvariant();
        if (_sections.Any(item => string.Equals(item.Code, normalizedCode, StringComparison.OrdinalIgnoreCase)))
            return Result.Failure<Guid>(new Error("Archive.DuplicateSectionCode", "El código de sección ya existe en este Fondo."));

        var section = new ArchiveSection(normalizedCode, name.Trim());
        _sections.Add(section);
        MarkAsUpdated();
        AddDomainEvent(new SectionCreated(Id, section.Id, section.Code, section.Name));
        return Result.Success(section.Id);
    }

    public Result<Guid> AddSeries(Guid sectionId, string code, string name)
    {
        if (sectionId == Guid.Empty)
            return Result.Failure<Guid>(new Error("Archive.SectionRequired", "La sección documental es obligatoria."));

        var validation = ValidateCodeAndName(code, name);
        if (validation.IsFailure)
            return Result.Failure<Guid>(validation.Error);

        var section = _sections.FirstOrDefault(item => item.Id == sectionId);
        if (section is null)
            return Result.Failure<Guid>(new Error("Archive.SectionNotFound", "La sección no pertenece a este Fondo."));

        var normalizedCode = code.Trim().ToUpperInvariant();
        var result = section.AddSeries(normalizedCode, name.Trim());
        if (result.IsFailure)
            return result;

        MarkAsUpdated();
        AddDomainEvent(new SeriesCreated(Id, sectionId, result.Value, normalizedCode, name.Trim()));
        return result;
    }

    public Result<Guid> AddSubseries(Guid seriesId, string code, string name)
    {
        if (seriesId == Guid.Empty)
            return Result.Failure<Guid>(new Error("Archive.SeriesRequired", "La serie documental es obligatoria."));

        var validation = ValidateCodeAndName(code, name);
        if (validation.IsFailure)
            return Result.Failure<Guid>(validation.Error);

        var series = _sections.SelectMany(item => item.Series).FirstOrDefault(item => item.Id == seriesId);
        if (series is null)
            return Result.Failure<Guid>(new Error("Archive.SeriesNotFound", "La serie no pertenece a este Fondo."));

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

        var subseries = _sections.SelectMany(section => section.Series)
            .SelectMany(series => series.Subseries)
            .FirstOrDefault(item => item.Id == subseriesId);
        if (subseries is null)
            return Result.Failure<Guid>(new Error("Archive.SubseriesNotFound", "La subserie no pertenece a este Fondo."));

        var normalizedReference = reference.Trim();
        var result = subseries.AddExpedient(normalizedReference, title.Trim());
        if (result.IsFailure)
            return result;

        MarkAsUpdated();
        AddDomainEvent(new ExpedientCreated(Id, subseriesId, result.Value, normalizedReference, title.Trim()));
        return result;
    }

    public bool ContainsExpedient(Guid expedientId)
        => expedientId != Guid.Empty
           && _sections.SelectMany(section => section.Series)
               .SelectMany(series => series.Subseries)
               .Any(subseries => subseries.ContainsExpedient(expedientId));

    private static Result ValidateCodeAndName(string code, string name)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Trim().Length > 50)
            return Result.Failure(new Error("Archive.InvalidClassificationCode", "El código de clasificación es obligatorio y no puede superar 50 caracteres."));
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 250)
            return Result.Failure(new Error("Archive.InvalidClassificationName", "El nombre de clasificación es obligatorio y no puede superar 250 caracteres."));
        return Result.Success();
    }
}
