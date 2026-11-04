namespace ArchiCore.Domain.Digitization;

/// <summary>Metadata supplied by the application layer after binary storage succeeds.</summary>
public sealed record DigitizedFileInput(
    string StorageObjectKey,
    string OriginalFileName,
    string ContentType,
    long SizeBytes,
    string Sha256,
    int PageCount);
