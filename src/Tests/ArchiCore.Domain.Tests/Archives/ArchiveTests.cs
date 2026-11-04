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
    public void Classification_hierarchy_requires_parent_and_enforces_unique_codes()
    {
        var archive = Archive.Create(Guid.NewGuid()).Value;
        var invalidSubseries = archive.AddSubseries(Guid.NewGuid(), "S-01", "Subserie");
        Assert.True(invalidSubseries.IsFailure);
        Assert.Equal("Archive.SeriesNotFound", invalidSubseries.Error.Code);

        var seriesId = archive.AddSeries("s-01", "Serie documental").Value;
        var duplicateSeries = archive.AddSeries("S-01", "Duplicate");
        Assert.True(duplicateSeries.IsFailure);
        Assert.Equal("Archive.DuplicateSeriesCode", duplicateSeries.Error.Code);

        var subseriesId = archive.AddSubseries(seriesId, "SS-01", "Subserie documental").Value;
        var expedientId = archive.AddExpedient(subseriesId, "EXP-2026-001", "Expediente contractual").Value;
        Assert.True(archive.ContainsExpedient(expedientId));
        Assert.Contains(archive.DomainEvents, item => item is SeriesCreated);
        Assert.Contains(archive.DomainEvents, item => item is SubseriesCreated);
        Assert.Contains(archive.DomainEvents, item => item is ExpedientCreated);
    }

    [Fact]
    public void Duplicate_expedient_reference_is_rejected_within_subseries()
    {
        var archive = Archive.Create(Guid.NewGuid()).Value;
        var series = archive.AddSeries("S-01", "Serie").Value;
        var subseries = archive.AddSubseries(series, "SS-01", "Subserie").Value;
        Assert.True(archive.AddExpedient(subseries, "EXP-001", "Uno").IsSuccess);

        var duplicate = archive.AddExpedient(subseries, "exp-001", "Duplicado");

        Assert.True(duplicate.IsFailure);
        Assert.Equal("Archive.DuplicateExpedientReference", duplicate.Error.Code);
    }
}
