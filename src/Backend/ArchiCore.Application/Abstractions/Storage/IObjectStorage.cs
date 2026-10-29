namespace ArchiCore.Application.Abstractions.Storage;

/// <summary>
/// Stores document bytes independently from business metadata and document lifecycle.
/// Callers must persist the returned/business object key in an approved metadata model before
/// exposing any read or delete operation through the API.
/// </summary>
public interface IObjectStorage
{
    Task StoreAsync(
        string objectKey,
        Stream content,
        long contentLength,
        string contentType,
        CancellationToken cancellationToken = default);

    Task CopyToAsync(
        string objectKey,
        Stream destination,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string objectKey,
        CancellationToken cancellationToken = default);
}
