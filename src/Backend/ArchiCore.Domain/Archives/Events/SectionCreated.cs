using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.Archives.Events;

public sealed class SectionCreated : DomainEvent
{
    public Guid ArchiveId { get; }
    public Guid SectionId { get; }
    public string Code { get; }
    public string Name { get; }

    public SectionCreated(Guid archiveId, Guid sectionId, string code, string name)
    {
        ArchiveId = archiveId;
        SectionId = sectionId;
        Code = code;
        Name = name;
    }
}
