using ArchiCore.Domain.Common;
using ArchiCore.Domain.Folders.Events;
using ArchiCore.Domain.ValueObjects;

namespace ArchiCore.Domain.Folders;

/// <summary>
/// Aggregate Root de la carpeta física. Expediente y caja se referencian por identidad;
/// documentos, folios y anexos son entidades internas.
/// </summary>
public sealed class Folder : Entity, IAggregateRoot
{
    private readonly List<FolderDocument> _documents = new();

    public FolderCode FolderCode { get; private set; }
    public Guid ArchiveId { get; private set; }
    public Guid ExpedientId { get; private set; }
    public Guid? BoxId { get; private set; }
    public Guid? LoanId { get; private set; }
    public FolderStatus Status { get; private set; }
    public IReadOnlyCollection<FolderDocument> Documents => _documents.AsReadOnly();

    private Folder() => FolderCode = null!;

    private Folder(FolderCode folderCode, Guid archiveId, Guid expedientId)
    {
        FolderCode = folderCode;
        ArchiveId = archiveId;
        ExpedientId = expedientId;
        Status = FolderStatus.Created;
    }

    public static Result<Folder> Create(FolderCode folderCode, Guid archiveId, Guid expedientId)
    {
        if (folderCode is null)
            return Result.Failure<Folder>(new Error("Folder.FolderCodeRequired", "El código de carpeta es obligatorio."));
        if (archiveId == Guid.Empty)
            return Result.Failure<Folder>(new Error("Folder.ArchiveRequired", "El archivo documental es obligatorio."));
        if (expedientId == Guid.Empty)
            return Result.Failure<Folder>(new Error("Folder.ExpedientRequired", "El expediente asociado es obligatorio."));

        var folder = new Folder(folderCode, archiveId, expedientId);
        folder.AddDomainEvent(new FolderCreated(folder.Id, folder.FolderCode.Value, archiveId, expedientId));
        return Result.Success(folder);
    }

    public Result Receive()
    {
        var validation = EnsureStatusIs(FolderStatus.Created, "Folder.InvalidReception", "Solo una carpeta Created puede recibirse.");
        if (validation.IsFailure) return validation;
        Status = FolderStatus.Received;
        MarkAsUpdated();
        AddDomainEvent(new FolderReceived(Id));
        return Result.Success();
    }

    public Result Organize()
    {
        var validation = EnsureStatusIs(FolderStatus.Received, "Folder.InvalidOrganization", "Solo una carpeta Received puede organizarse.");
        if (validation.IsFailure) return validation;
        Status = FolderStatus.Organized;
        MarkAsUpdated();
        AddDomainEvent(new FolderOrganized(Id));
        return Result.Success();
    }

    public Result<Guid> AddDocument(string title, DateTime? documentDate = null)
    {
        if (Status is not (FolderStatus.Received or FolderStatus.Organized))
            return Result.Failure<Guid>(new Error("Folder.InvalidDocumentRegistrationState", "Los documentos solo pueden registrarse en una carpeta Received u Organized."));
        if (string.IsNullOrWhiteSpace(title) || title.Trim().Length > 300)
            return Result.Failure<Guid>(new Error("Folder.InvalidDocumentTitle", "El título del documento es obligatorio y no puede superar 300 caracteres."));

        var document = new FolderDocument(title.Trim(), documentDate);
        _documents.Add(document);
        MarkAsUpdated();
        AddDomainEvent(new DocumentRegistered(Id, document.Id, document.Title));
        return Result.Success(document.Id);
    }

    public Result<int> AddFolio(Guid documentId, int number)
    {
        if (Status is not (FolderStatus.Received or FolderStatus.Organized))
            return Result.Failure<int>(new Error("Folder.InvalidFoliationState", "La foliación solo puede modificarse en una carpeta Received u Organized."));
        var document = _documents.FirstOrDefault(item => item.Id == documentId);
        if (document is null)
            return Result.Failure<int>(new Error("Folder.DocumentNotFound", "El documento no pertenece a esta carpeta."));

        var result = document.AddFolio(number);
        if (result.IsFailure) return result;
        MarkAsUpdated();
        AddDomainEvent(new FolioAdded(Id, documentId, result.Value));
        return result;
    }

    public Result<int> AddAnnex(Guid documentId, string title)
    {
        if (Status is not (FolderStatus.Received or FolderStatus.Organized))
            return Result.Failure<int>(new Error("Folder.InvalidAnnexRegistrationState", "Los anexos solo pueden agregarse en una carpeta Received u Organized."));
        var document = _documents.FirstOrDefault(item => item.Id == documentId);
        if (document is null)
            return Result.Failure<int>(new Error("Folder.DocumentNotFound", "El documento no pertenece a esta carpeta."));

        var result = document.AddAnnex(title);
        if (result.IsFailure) return result;
        MarkAsUpdated();
        AddDomainEvent(new AnnexAdded(Id, documentId, result.Value, title.Trim()));
        return result;
    }

    public Result Store(Guid boxId)
    {
        var validation = EnsureStatusIs(FolderStatus.Organized, "Folder.InvalidStorage", "Solo una carpeta Organized puede almacenarse en una caja.");
        if (validation.IsFailure) return validation;
        if (boxId == Guid.Empty)
            return Result.Failure(new Error("Folder.BoxRequired", "La caja de almacenamiento es obligatoria."));

        BoxId = boxId;
        Status = FolderStatus.Stored;
        MarkAsUpdated();
        AddDomainEvent(new FolderAssignedToBox(Id, boxId));
        return Result.Success();
    }

    public Result MoveToBox(Guid newBoxId)
    {
        var validation = EnsureStatusIs(FolderStatus.Stored, "Folder.InvalidMove", "Solo una carpeta Stored puede moverse entre cajas.");
        if (validation.IsFailure) return validation;
        if (newBoxId == Guid.Empty)
            return Result.Failure(new Error("Folder.BoxRequired", "La caja de destino es obligatoria."));
        if (BoxId == newBoxId)
            return Result.Failure(new Error("Folder.SameBox", "La carpeta ya pertenece a la caja indicada."));

        var previousBoxId = BoxId!.Value;
        BoxId = newBoxId;
        MarkAsUpdated();
        AddDomainEvent(new FolderMoved(Id, previousBoxId, newBoxId));
        return Result.Success();
    }

    public Result StartLoan(Guid loanId)
    {
        var validation = EnsureStatusIs(FolderStatus.Stored, "Folder.InvalidLoan", "Solo una carpeta Stored puede iniciar un préstamo.");
        if (validation.IsFailure) return validation;
        if (loanId == Guid.Empty)
            return Result.Failure(new Error("Folder.LoanRequired", "El préstamo asociado es obligatorio."));

        LoanId = loanId;
        Status = FolderStatus.Loaned;
        MarkAsUpdated();
        AddDomainEvent(new FolderLoaned(Id, loanId));
        return Result.Success();
    }

    public Result ReturnLoan()
    {
        var validation = EnsureStatusIs(FolderStatus.Loaned, "Folder.InvalidReturn", "Solo una carpeta Loaned puede devolverse.");
        if (validation.IsFailure) return validation;

        var loanId = LoanId!.Value;
        LoanId = null;
        Status = FolderStatus.Returned;
        MarkAsUpdated();
        AddDomainEvent(new FolderReturned(Id, loanId));
        return Result.Success();
    }

    public Result Archive()
    {
        var validation = EnsureStatusIs(FolderStatus.Returned, "Folder.InvalidArchive", "Solo una carpeta Returned puede archivarse.");
        if (validation.IsFailure) return validation;

        Status = FolderStatus.Archived;
        MarkAsUpdated();
        AddDomainEvent(new FolderArchived(Id));
        return Result.Success();
    }

    private Result EnsureStatusIs(FolderStatus expected, string errorCode, string errorMessage)
        => Status != expected ? Result.Failure(new Error(errorCode, errorMessage)) : Result.Success();
}
