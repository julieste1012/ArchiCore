using ArchiCore.Domain.Common;

namespace ArchiCore.Domain.Digitization;

/// <summary>Metadata for an object in IObjectStorage. File bytes are never held by the domain.</summary>
public sealed class DigitizedFile : Entity
{
    public string StorageObjectKey { get; private set; }
    public string OriginalFileName { get; private set; }
    public string ContentType { get; private set; }
    public long SizeBytes { get; private set; }
    public string Sha256 { get; private set; }
    public int PageCount { get; private set; }

    internal DigitizedFile(DigitizedFileInput input)
    {
        StorageObjectKey = input.StorageObjectKey.Trim();
        OriginalFileName = input.OriginalFileName.Trim();
        ContentType = input.ContentType.Trim().ToLowerInvariant();
        SizeBytes = input.SizeBytes;
        Sha256 = input.Sha256.Trim().ToLowerInvariant();
        PageCount = input.PageCount;
    }
}
