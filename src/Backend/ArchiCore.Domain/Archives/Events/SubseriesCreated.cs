using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.Archives.Events;

public sealed class SubseriesCreated : DomainEvent
{
    public Guid ArchiveId { get; }
    public Guid SeriesId { get; }
    public Guid SubseriesId { get; }
    public string Code { get; }
    public string Name { get; }

    public SubseriesCreated(Guid archiveId, Guid seriesId, Guid subseriesId, string code, string name)
    {
        ArchiveId = archiveId;
        SeriesId = seriesId;
        SubseriesId = subseriesId;
        Code = code;
        Name = name;
    }
}
