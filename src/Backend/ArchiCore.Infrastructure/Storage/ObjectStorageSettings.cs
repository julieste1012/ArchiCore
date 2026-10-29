using Microsoft.Extensions.Configuration;

namespace ArchiCore.Infrastructure.Storage;

public sealed class ObjectStorageSettings
{
    public const long DefaultMaxObjectSizeBytes = 100L * 1024 * 1024;

    public required string Provider { get; init; }
    public required string LocalPath { get; init; }
    public required long MaxObjectSizeBytes { get; init; }
    public required string Bucket { get; init; }
    public string? Endpoint { get; init; }
    public string? AccessKey { get; init; }
    public string? SecretKey { get; init; }
    public bool Secure { get; init; }

    public static ObjectStorageSettings FromConfiguration(IConfiguration configuration)
    {
        var provider = (configuration["ObjectStorage:Provider"] ?? "Local").Trim();
        if (!provider.Equals("Local", StringComparison.OrdinalIgnoreCase)
            && !provider.Equals("Minio", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "ObjectStorage:Provider must be either 'Local' or 'Minio'.");
        }

        var maxBytes = long.TryParse(configuration["ObjectStorage:MaxObjectSizeBytes"], out var configuredMax)
            ? configuredMax
            : DefaultMaxObjectSizeBytes;
        if (maxBytes <= 0)
        {
            throw new InvalidOperationException("ObjectStorage:MaxObjectSizeBytes must be positive.");
        }

        var bucket = configuration["ObjectStorage:Bucket"] ?? "archicore-documents";
        if (bucket.Length is < 3 or > 63
            || bucket.Any(character => !(char.IsAsciiLetterLower(character) || char.IsAsciiDigit(character) || character is '-' or '.'))
            || bucket[0] is '-' or '.'
            || bucket[^1] is '-' or '.'
            || bucket.Contains("..", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "ObjectStorage:Bucket must be a valid lowercase S3-compatible bucket name.");
        }

        var settings = new ObjectStorageSettings
        {
            Provider = provider,
            LocalPath = configuration["ObjectStorage:LocalPath"] ?? "App_Data/Objects",
            MaxObjectSizeBytes = maxBytes,
            Bucket = bucket,
            Endpoint = configuration["ObjectStorage:Endpoint"] ?? "127.0.0.1:9000",
            AccessKey = configuration["ObjectStorage:AccessKey"],
            SecretKey = configuration["ObjectStorage:SecretKey"],
            Secure = bool.TryParse(configuration["ObjectStorage:Secure"], out var secure) && secure
        };

        if (provider.Equals("Minio", StringComparison.OrdinalIgnoreCase)
            && (string.IsNullOrWhiteSpace(settings.AccessKey) || string.IsNullOrWhiteSpace(settings.SecretKey)))
        {
            throw new InvalidOperationException(
                "ObjectStorage:AccessKey and ObjectStorage:SecretKey are required when ObjectStorage:Provider is 'Minio'. Configure them through environment variables or user-secrets.");
        }

        return settings;
    }
}
