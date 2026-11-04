using ArchiCore.Domain.Common;
using ArchiCore.Domain.Digitization.Events;

namespace ArchiCore.Domain.Digitization;

/// <summary>
/// Aggregate Root que controla la solicitud y el resultado de digitalización de una carpeta.
/// Conserva metadatos y claves opacas; los bytes permanecen en IObjectStorage.
/// </summary>
public sealed class DigitizationRequest : Entity, IAggregateRoot
{
    private readonly List<DigitizedFile> _files = new();

    public Guid ProjectId { get; private set; }
    public Guid FolderId { get; private set; }
    public Guid RequestedByUserId { get; private set; }
    public DateTime RequestedAtUtc { get; private set; }
    public DigitizationRequestStatus Status { get; private set; }
    public Guid? StartedByUserId { get; private set; }
    public DateTime? StartedAtUtc { get; private set; }
    public Guid? CompletedByUserId { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }
    public IReadOnlyCollection<DigitizedFile> Files => _files.AsReadOnly();

    private DigitizationRequest() { }

    private DigitizationRequest(Guid projectId, Guid folderId, Guid requestedByUserId)
    {
        ProjectId = projectId;
        FolderId = folderId;
        RequestedByUserId = requestedByUserId;
        RequestedAtUtc = DateTime.UtcNow;
        Status = DigitizationRequestStatus.Requested;
    }

    public static Result<DigitizationRequest> Create(Guid projectId, Guid folderId, Guid requestedByUserId)
    {
        if (projectId == Guid.Empty)
            return Result.Failure<DigitizationRequest>(new Error("Digitization.ProjectRequired", "El proyecto asociado es obligatorio."));
        if (folderId == Guid.Empty)
            return Result.Failure<DigitizationRequest>(new Error("Digitization.FolderRequired", "La carpeta que se va a digitalizar es obligatoria."));
        if (requestedByUserId == Guid.Empty)
            return Result.Failure<DigitizationRequest>(new Error("Digitization.RequesterRequired", "El usuario solicitante es obligatorio."));

        var request = new DigitizationRequest(projectId, folderId, requestedByUserId);
        request.AddDomainEvent(new DigitizationRequested(request.Id, projectId, folderId, requestedByUserId));
        return Result.Success(request);
    }

    public Result Start(Guid startedByUserId)
    {
        if (Status != DigitizationRequestStatus.Requested)
            return Result.Failure(new Error("Digitization.InvalidStartState", "Solo una solicitud Requested puede iniciar el proceso de digitalización."));
        if (startedByUserId == Guid.Empty)
            return Result.Failure(new Error("Digitization.OperatorRequired", "El usuario responsable de iniciar la digitalización es obligatorio."));

        StartedByUserId = startedByUserId;
        StartedAtUtc = DateTime.UtcNow;
        Status = DigitizationRequestStatus.Processing;
        MarkAsUpdated();
        AddDomainEvent(new DigitizationStarted(Id, FolderId, startedByUserId));
        return Result.Success();
    }

    public Result Complete(Guid completedByUserId, IReadOnlyCollection<DigitizedFileInput>? files)
    {
        if (Status != DigitizationRequestStatus.Processing)
            return Result.Failure(new Error("Digitization.InvalidCompletionState", "La solicitud debe estar en Processing antes de completarse."));
        if (completedByUserId == Guid.Empty)
            return Result.Failure(new Error("Digitization.OperatorRequired", "El usuario responsable de completar la digitalización es obligatorio."));
        if (files is null || files.Count == 0)
            return Result.Failure(new Error("Digitization.FilesRequired", "La digitalización debe generar al menos un archivo digital."));

        var validation = ValidateFiles(files);
        if (validation.IsFailure)
            return validation;

        foreach (var input in files)
        {
            var file = new DigitizedFile(input);
            _files.Add(file);
            AddDomainEvent(new DigitalFileGenerated(Id, file.Id, file.StorageObjectKey, file.SizeBytes, file.PageCount));
        }

        CompletedByUserId = completedByUserId;
        CompletedAtUtc = DateTime.UtcNow;
        Status = DigitizationRequestStatus.Completed;
        MarkAsUpdated();
        AddDomainEvent(new DigitizationCompleted(Id, FolderId, completedByUserId, _files.Count));
        return Result.Success();
    }

    private static Result ValidateFiles(IReadOnlyCollection<DigitizedFileInput> files)
    {
        var objectKeys = new HashSet<string>(StringComparer.Ordinal);

        foreach (var file in files)
        {
            if (file is null)
                return Result.Failure(new Error("Digitization.InvalidFile", "La lista contiene un archivo nulo."));

            if (string.IsNullOrWhiteSpace(file.StorageObjectKey)
                || file.StorageObjectKey.Length > 512
                || file.StorageObjectKey.StartsWith("/", StringComparison.Ordinal)
                || file.StorageObjectKey.Contains('\\')
                || file.StorageObjectKey.Split('/').Any(segment => segment is "." or ".." or ""))
                return Result.Failure(new Error("Digitization.InvalidObjectKey", "La clave del objeto almacenado no tiene un formato relativo seguro."));

            if (!objectKeys.Add(file.StorageObjectKey))
                return Result.Failure(new Error("Digitization.DuplicateObjectKey", "Una misma clave de almacenamiento no puede registrarse más de una vez en la solicitud."));

            if (string.IsNullOrWhiteSpace(file.OriginalFileName) || file.OriginalFileName.Trim().Length > 255)
                return Result.Failure(new Error("Digitization.InvalidFileName", "El nombre original del archivo es obligatorio y no puede superar 255 caracteres."));

            if (string.IsNullOrWhiteSpace(file.ContentType)
                || file.ContentType.Trim().Length > 128
                || !file.ContentType.Contains('/'))
                return Result.Failure(new Error("Digitization.InvalidContentType", "El tipo MIME debe estar informado y tener el formato tipo/subtipo."));

            if (file.SizeBytes <= 0)
                return Result.Failure(new Error("Digitization.InvalidFileSize", "El tamaño del archivo debe ser mayor que cero."));
            if (file.PageCount <= 0)
                return Result.Failure(new Error("Digitization.InvalidPageCount", "El número de páginas debe ser mayor que cero."));
            if (file.Sha256 is null || file.Sha256.Length != 64 || !file.Sha256.All(Uri.IsHexDigit))
                return Result.Failure(new Error("Digitization.InvalidChecksum", "El checksum SHA-256 debe contener exactamente 64 caracteres hexadecimales."));
        }

        return Result.Success();
    }
}
