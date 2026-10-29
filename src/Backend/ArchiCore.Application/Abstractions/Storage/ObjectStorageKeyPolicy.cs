using System.Text;

namespace ArchiCore.Application.Abstractions.Storage;

/// <summary>
/// Common object-key rules enforced by every provider. Keys are opaque storage identifiers,
/// never client-supplied file-system paths or original file names.
/// </summary>
public static class ObjectStorageKeyPolicy
{
    public static string Validate(string objectKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(objectKey);

        if (Encoding.UTF8.GetByteCount(objectKey) > 1024)
        {
            throw new ArgumentException("Object key exceeds the 1024-byte storage limit.", nameof(objectKey));
        }

        if (objectKey.StartsWith("/", StringComparison.Ordinal)
            || objectKey.EndsWith("/", StringComparison.Ordinal)
            || objectKey.Contains('\\')
            || objectKey.Contains('?')
            || objectKey.Contains('#')
            || objectKey.Any(char.IsControl))
        {
            throw new ArgumentException("Object key contains a forbidden character or path boundary.", nameof(objectKey));
        }

        var segments = objectKey.Split('/');
        if (segments.Any(segment => segment.Length == 0 || segment is "." or ".."))
        {
            throw new ArgumentException("Object key contains an empty or traversal path segment.", nameof(objectKey));
        }

        return objectKey;
    }
}
