using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.Archives.Events;

public sealed class ExpedientCreated : DomainEvent
{
    public Guid ArchiveId { get; }
    public Guid SubseriesId { get; }
    public Guid ExpedientId { get; }
    public string Reference { get; }
    public string Title { get; }

    public ExpedientCreated(Guid archiveId, Guid subseriesId, Guid expedientId, string reference, string title)
    {
        ArchiveId = archiveId;
        SubseriesId = subseriesId;
        ExpedientId = expedientId;
        Reference = reference;
        Title = title;
    }
}
