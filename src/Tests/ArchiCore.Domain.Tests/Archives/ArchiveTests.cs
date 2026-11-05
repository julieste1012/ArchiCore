using ArchiCore.Domain.Archives;
using ArchiCore.Domain.Archives.Events;
using Xunit;

namespace ArchiCore.Domain.Tests.Archives;

public sealed class ArchiveTests
{
    [Fact]
    public void Create_requires_a_project_and_emits_archive_created()
    {
        var invalid = Archive.Create(Guid.Empty);
        Assert.True(invalid.IsFailure);
        Assert.Equal("Archive.ProjectRequired", invalid.Error.Code);

        var archive = Archive.Create(Guid.NewGuid()).Value;
        Assert.IsType<ArchiveCreated>(Assert.Single(archive.DomainEvents));
    }

    [Fact]
    public void Archival_classification_follows_fund_section_series_subseries_expedient()
    {
        var archive = Archive.Create(Guid.NewGuid()).Value;
        var invalidSeries = archive.AddSeries(Guid.NewGuid(), "S-01", "Serie");
        Assert.True(invalidSeries.IsFailure);
        Assert.Equal("Archive.SectionNotFound", invalidSeries.Error.Code);

        var sectionId = archive.AddSection("SEC-01", "Sección Administrativa").Value;
        var duplicateSection = archive.AddSection("sec-01", "Duplicada");
        Assert.True(duplicateSection.IsFailure);
        Assert.Equal("Archive.DuplicateSectionCode", duplicateSection.Error.Code);

        var seriesId = archive.AddSeries(sectionId, "S-01", "Serie documental").Value;
        var duplicateSeries = archive.AddSeries(sectionId, "s-01", "Duplicada");
        Assert.True(duplicateSeries.IsFailure);
        Assert.Equal("Archive.DuplicateSeriesCode", duplicateSeries.Error.Code);

        var subseriesId = archive.AddSubseries(seriesId, "SS-01", "Subserie documental").Value;
        var expedientId = archive.AddExpedient(subseriesId, "EXP-2026-001", "Expediente contractual").Value;

        Assert.True(archive.ContainsExpedient(expedientId));
        Assert.Contains(archive.DomainEvents, item => item is SectionCreated);
        Assert.Contains(archive.DomainEvents, item => item is SeriesCreated);
        Assert.Contains(archive.DomainEvents, item => item is SubseriesCreated);
        Assert.Contains(archive.DomainEvents, item => item is ExpedientCreated);
    }

    [Fact]
    public void Duplicate_expedient_reference_is_rejected_within_subseries()
    {
        var archive = Archive.Create(Guid.NewGuid()).Value;
        var section = archive.AddSection("SEC-01", "Sección").Value;
        var series = archive.AddSeries(section, "S-01", "Serie").Value;
        var subseries = archive.AddSubseries(series, "SS-01", "Subserie").Value;
        Assert.True(archive.AddExpedient(subseries, "EXP-001", "Uno").IsSuccess);

        var duplicate = archive.AddExpedient(subseries, "exp-001", "Duplicado");

        Assert.True(duplicate.IsFailure);
        Assert.Equal("Archive.DuplicateExpedientReference", duplicate.Error.Code);
    }
}
