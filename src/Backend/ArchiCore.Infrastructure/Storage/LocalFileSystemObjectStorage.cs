using ArchiCore.Application.Abstractions.Storage;

namespace ArchiCore.Infrastructure.Storage;

/// <summary>
/// Development-only storage provider. Objects stay under a configured root directory;
/// traversal is rejected, uploads use a temporary file, and the declared size is verified.
/// Production deployments should configure an approved S3-compatible provider.
/// </summary>
public sealed class LocalFileSystemObjectStorage : IObjectStorage
{
    private readonly string _rootPath;
    private readonly long _maxObjectSizeBytes;

    public LocalFileSystemObjectStorage(string rootPath, long maxObjectSizeBytes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rootPath);
        if (maxObjectSizeBytes <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxObjectSizeBytes));
        }

        _rootPath = Path.GetFullPath(rootPath);
        _maxObjectSizeBytes = maxObjectSizeBytes;
        Directory.CreateDirectory(_rootPath);
    }

    public async Task StoreAsync(
        string objectKey,
        Stream content,
        long contentLength,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var targetPath = ResolvePath(objectKey);
        ValidateUpload(content, contentLength, contentType);

        var parent = Path.GetDirectoryName(targetPath)
            ?? throw new InvalidOperationException("The object key has no parent directory.");
        Directory.CreateDirectory(parent);

        var temporaryPath = Path.Combine(parent, $".{Path.GetFileName(targetPath)}.{Guid.NewGuid():N}.tmp");
        try
        {
            await using (var output = new FileStream(
                temporaryPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920,
                options: FileOptions.Asynchronous | FileOptions.SequentialScan))
            {
                await content.CopyToAsync(output, cancellationToken);
                await output.FlushAsync(cancellationToken);

                if (output.Length != contentLength)
                {
                    throw new InvalidDataException(
                        $"Declared content length ({contentLength}) does not match uploaded bytes ({output.Length}).");
                }
            }

            File.Move(temporaryPath, targetPath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }

    public async Task CopyToAsync(
        string objectKey,
        Stream destination,
        CancellationToken cancellationToken = default)
    {
        var path = ResolvePath(objectKey);
        ArgumentNullException.ThrowIfNull(destination);
        if (!destination.CanWrite)
        {
            throw new ArgumentException("Destination stream must be writable.", nameof(destination));
        }

        await using var source = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 81920,
            options: FileOptions.Asynchronous | FileOptions.SequentialScan);

        await source.CopyToAsync(destination, cancellationToken);
    }

    public Task DeleteAsync(string objectKey, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var path = ResolvePath(objectKey);
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        return Task.CompletedTask;
    }

    private string ResolvePath(string objectKey)
    {
        ObjectStorageKeyPolicy.Validate(objectKey);
        var relative = objectKey.Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.GetFullPath(Path.Combine(_rootPath, relative));
        var relativeToRoot = Path.GetRelativePath(_rootPath, fullPath);

        if (Path.IsPathRooted(relativeToRoot)
            || relativeToRoot.Equals("..", StringComparison.Ordinal)
            || relativeToRoot.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
        {
            throw new ArgumentException("Object key escapes the configured storage root.", nameof(objectKey));
        }

        return fullPath;
    }

    private void ValidateUpload(Stream content, long contentLength, string contentType)
    {
        ArgumentNullException.ThrowIfNull(content);
        if (!content.CanRead)
        {
            throw new ArgumentException("Upload stream must be readable.", nameof(content));
        }

        if (contentLength < 0 || contentLength > _maxObjectSizeBytes)
        {
            throw new ArgumentOutOfRangeException(
                nameof(contentLength),
                $"Content length must be between 0 and {_maxObjectSizeBytes} bytes.");
        }

        if (string.IsNullOrWhiteSpace(contentType)
            || contentType.Length > 255
            || contentType.Any(char.IsControl))
        {
            throw new ArgumentException("A valid content type is required.", nameof(contentType));
        }
    }
}
