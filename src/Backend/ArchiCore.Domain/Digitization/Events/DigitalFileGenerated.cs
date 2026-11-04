using ArchiCore.Domain.Events;

namespace ArchiCore.Domain.Digitization.Events;

public sealed class DigitalFileGenerated : DomainEvent
{
    public Guid RequestId { get; }
    public Guid DigitalFileId { get; }
    public string StorageObjectKey { get; }
    public long SizeBytes { get; }
    public int PageCount { get; }

    public DigitalFileGenerated(Guid requestId, Guid digitalFileId, string storageObjectKey, long sizeBytes, int pageCount)
    {
        RequestId = requestId;
        DigitalFileId = digitalFileId;
        StorageObjectKey = storageObjectKey;
        SizeBytes = sizeBytes;
        PageCount = pageCount;
    }
}
