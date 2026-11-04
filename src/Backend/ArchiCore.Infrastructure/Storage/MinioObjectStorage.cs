using ArchiCore.Application.Abstractions.Storage;
using ArchiCore.Infrastructure.Storage;
using Minio;
using Minio.DataModel.Args;

namespace ArchiCore.Infrastructure.Storage;

/// <summary>
/// S3-compatible object-storage adapter using the MinIO .NET SDK.
/// The client endpoint can point to MinIO, AIStor, or another compatible implementation.
/// </summary>
public sealed class MinioObjectStorage(
    IMinioClient client,
    ObjectStorageSettings settings) : IObjectStorage
{
    private readonly SemaphoreSlim _bucketInitializationLock = new(1, 1);
    private volatile bool _bucketReady;

    public async Task StoreAsync(
        string objectKey,
        Stream content,
        long contentLength,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        ObjectStorageKeyPolicy.Validate(objectKey);
        ValidateUpload(content, contentLength, contentType);
        await EnsureBucketExistsAsync(cancellationToken);

        var args = new PutObjectArgs()
            .WithBucket(settings.Bucket)
            .WithObject(objectKey)
            .WithStreamData(content)
            .WithObjectSize(contentLength)
            .WithContentType(contentType);

        await client.PutObjectAsync(args, cancellationToken);
    }

    public async Task CopyToAsync(
        string objectKey,
        Stream destination,
        CancellationToken cancellationToken = default)
    {
        ObjectStorageKeyPolicy.Validate(objectKey);
        ArgumentNullException.ThrowIfNull(destination);
        if (!destination.CanWrite)
        {
            throw new ArgumentException("Destination stream must be writable.", nameof(destination));
        }

        await EnsureBucketExistsAsync(cancellationToken);
        var args = new GetObjectArgs()
            .WithBucket(settings.Bucket)
            .WithObject(objectKey)
            .WithCallbackStream((stream, token) => stream.CopyToAsync(destination, token));

        await client.GetObjectAsync(args, cancellationToken);
    }

    public async Task DeleteAsync(
        string objectKey,
        CancellationToken cancellationToken = default)
    {
        ObjectStorageKeyPolicy.Validate(objectKey);
        await EnsureBucketExistsAsync(cancellationToken);
        await client.RemoveObjectAsync(
            new RemoveObjectArgs().WithBucket(settings.Bucket).WithObject(objectKey),
            cancellationToken);
    }

    private async Task EnsureBucketExistsAsync(CancellationToken cancellationToken)
    {
        if (_bucketReady)
        {
            return;
        }

        await _bucketInitializationLock.WaitAsync(cancellationToken);
        try
        {
            if (_bucketReady)
            {
                return;
            }

            var exists = await client.BucketExistsAsync(
                new BucketExistsArgs().WithBucket(settings.Bucket),
                cancellationToken);

            if (!exists)
            {
                try
                {
                    await client.MakeBucketAsync(
                        new MakeBucketArgs().WithBucket(settings.Bucket),
                        cancellationToken);
                }
                catch (Exception) when (!cancellationToken.IsCancellationRequested)
                {
                    // Another API instance may have created the bucket after the first check.
                    // Suppress only if the bucket is now present; otherwise preserve the failure.
                    if (!await client.BucketExistsAsync(
                            new BucketExistsArgs().WithBucket(settings.Bucket),
                            cancellationToken))
                    {
                        throw;
                    }
                }
            }

            _bucketReady = true;
        }
        finally
        {
            _bucketInitializationLock.Release();
        }
    }

    private void ValidateUpload(Stream content, long contentLength, string contentType)
    {
        ArgumentNullException.ThrowIfNull(content);
        if (!content.CanRead)
        {
            throw new ArgumentException("Upload stream must be readable.", nameof(content));
        }

        if (contentLength < 0 || contentLength > settings.MaxObjectSizeBytes)
        {
            throw new ArgumentOutOfRangeException(
                nameof(contentLength),
                $"Content length must be between 0 and {settings.MaxObjectSizeBytes} bytes.");
        }

        if (string.IsNullOrWhiteSpace(contentType)
            || contentType.Length > 255
            || contentType.Any(char.IsControl))
        {
            throw new ArgumentException("A valid content type is required.", nameof(contentType));
        }
    }
}
