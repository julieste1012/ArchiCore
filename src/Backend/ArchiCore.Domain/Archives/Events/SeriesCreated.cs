using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.Archives.Events;

public sealed class SeriesCreated : DomainEvent
{
    public Guid ArchiveId { get; }
    public Guid SectionId { get; }
    public Guid SeriesId { get; }
    public string Code { get; }
    public string Name { get; }

    public SeriesCreated(Guid archiveId, Guid sectionId, Guid seriesId, string code, string name)
    {
        ArchiveId = archiveId;
        SectionId = sectionId;
        SeriesId = seriesId;
        Code = code;
        Name = name;
    }
}
