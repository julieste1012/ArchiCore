using ArchiCore.Application.Abstractions.Persistence;
using ArchiCore.Domain.Archives;
using ArchiCore.Domain.Boxes;
using ArchiCore.Domain.Common;
using ArchiCore.Domain.Digitization;
using ArchiCore.Domain.Folders;
using ArchiCore.Domain.Loans;
using ArchiCore.Domain.ValueObjects;

namespace ArchiCore.Application.Documentary;

/// <summary>
/// Coordinates operations spanning multiple aggregate roots. Each cross-aggregate command
/// mutates all participating aggregates before a single SaveChangesAsync call so the relational
/// provider commits the changes and their outbox events in one transaction.
/// </summary>
public sealed class DocumentaryWorkflowService(
    IProjectRepository projects,
    IArchiveRepository archives,
    IFolderRepository folders,
    IBoxRepository boxes,
    ILoanRepository loans,
    IDigitizationRequestRepository digitizationRequests,
    IUnitOfWork unitOfWork)
{
    public async Task<Result<Guid>> CreateArchiveAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        if (await projects.GetByIdAsync(projectId, cancellationToken) is null)
            return Result.Failure<Guid>(new Error("Archive.ProjectNotFound", "El proyecto indicado no existe."));

        if (await archives.GetByProjectIdAsync(projectId, cancellationToken) is not null)
            return Result.Failure<Guid>(new Error("Archive.AlreadyExists", "El proyecto ya tiene un Fondo documental registrado."));

        var result = Archive.Create(projectId);
        if (result.IsFailure) return Result.Failure<Guid>(result.Error);

        await archives.AddAsync(result.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(result.Value.Id);
    }

    public async Task<Result<Guid>> CreateFolderAsync(
        string folderCode,
        Guid archiveId,
        Guid expedientId,
        CancellationToken cancellationToken = default)
    {
        var code = FolderCode.Create(folderCode);
        if (code.IsFailure) return Result.Failure<Guid>(code.Error);

        if (await folders.GetByCodeAsync(code.Value, cancellationToken) is not null)
            return Result.Failure<Guid>(new Error("Folder.DuplicateCode", "Ya existe una carpeta con ese código."));

        var archive = await archives.GetByIdAsync(archiveId, cancellationToken);
        if (archive is null)
            return Result.Failure<Guid>(new Error("Folder.ArchiveNotFound", "No se encontró el Fondo documental indicado."));

        if (!archive.ContainsExpedient(expedientId))
            return Result.Failure<Guid>(new Error("Folder.ExpedientNotInArchive", "El expediente no pertenece al Fondo documental indicado."));

        var result = Folder.Create(code.Value, archiveId, expedientId);
        if (result.IsFailure) return Result.Failure<Guid>(result.Error);

        await folders.AddAsync(result.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(result.Value.Id);
    }

    public async Task<Result> AssignFolderToBoxAsync(
        Guid folderId,
        Guid boxId,
        CancellationToken cancellationToken = default)
    {
        var folder = await folders.GetByIdAsync(folderId, cancellationToken);
        if (folder is null)
            return Result.Failure(new Error("Folder.NotFound", "No se encontró la carpeta indicada."));

        var box = await boxes.GetByIdAsync(boxId, cancellationToken);
        if (box is null)
            return Result.Failure(new Error("Box.NotFound", "No se encontró la caja indicada."));

        if (folder.Status != FolderStatus.Organized || folder.BoxId is not null)
            return Result.Failure(new Error("Folder.NotReadyForStorage", "La carpeta debe estar organizada y no puede pertenecer ya a una caja."));
        if (box.Status != BoxStatus.InStorage)
            return Result.Failure(new Error("Box.NotInStorage", "La caja debe estar en almacenamiento para recibir carpetas."));
        if (box.FolderIds.Contains(folderId))
            return Result.Failure(new Error("Box.FolderAlreadyAssigned", "La caja ya contiene la carpeta indicada."));

        var boxResult = box.AddFolder(folderId);
        if (boxResult.IsFailure) return boxResult;

        var folderResult = folder.Store(boxId);
        if (folderResult.IsFailure) return folderResult;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> MoveFolderToBoxAsync(
        Guid folderId,
        Guid destinationBoxId,
        CancellationToken cancellationToken = default)
    {
        var folder = await folders.GetByIdAsync(folderId, cancellationToken);
        if (folder is null)
            return Result.Failure(new Error("Folder.NotFound", "No se encontró la carpeta indicada."));
        if (folder.Status != FolderStatus.Stored || folder.BoxId is null)
            return Result.Failure(new Error("Folder.NotStored", "La carpeta debe estar almacenada en una caja antes de trasladarse."));
        if (folder.BoxId == destinationBoxId)
            return Result.Failure(new Error("Folder.SameBox", "La carpeta ya pertenece a la caja indicada."));

        var sourceBox = await boxes.GetByIdAsync(folder.BoxId.Value, cancellationToken);
        if (sourceBox is null)
            return Result.Failure(new Error("Folder.SourceBoxNotFound", "No se encontró la caja donde está registrada la carpeta."));

        var destinationBox = await boxes.GetByIdAsync(destinationBoxId, cancellationToken);
        if (destinationBox is null)
            return Result.Failure(new Error("Box.NotFound", "No se encontró la caja de destino."));
        if (sourceBox.Status != BoxStatus.InStorage || destinationBox.Status != BoxStatus.InStorage)
            return Result.Failure(new Error("Box.NotInStorage", "Tanto la caja de origen como la de destino deben estar en almacenamiento."));
        if (!sourceBox.FolderIds.Contains(folderId))
            return Result.Failure(new Error("Folder.SourceBoxReferenceMissing", "La caja de origen no tiene registrada la carpeta."));
        if (destinationBox.FolderIds.Contains(folderId))
            return Result.Failure(new Error("Folder.DestinationAlreadyContainsFolder", "La caja de destino ya tiene registrada la carpeta."));

        var release = sourceBox.RemoveFolder(folderId);
        if (release.IsFailure) return release;

        var add = destinationBox.AddFolder(folderId);
        if (add.IsFailure) return add;

        var move = folder.MoveToBox(destinationBoxId);
        if (move.IsFailure) return move;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<Guid>> RequestLoanAsync(
        Guid projectId,
        Guid folderId,
        Guid requestedByUserId,
        DateTimeOffset dueAtUtc,
        CancellationToken cancellationToken = default)
    {
        var archive = await archives.GetByProjectIdAsync(projectId, cancellationToken);
        if (archive is null)
            return Result.Failure<Guid>(new Error("Loan.ProjectArchiveNotFound", "El proyecto no tiene un Fondo documental registrado."));

        var folder = await folders.GetByIdAsync(folderId, cancellationToken);
        if (folder is null || folder.ArchiveId != archive.Id)
            return Result.Failure<Guid>(new Error("Loan.FolderNotInProject", "La carpeta no pertenece al proyecto indicado."));
        if (folder.Status != FolderStatus.Stored)
            return Result.Failure<Guid>(new Error("Loan.FolderNotAvailable", "Solo se pueden solicitar carpetas actualmente almacenadas."));

        if (await loans.GetActiveByFolderAsync(folderId, cancellationToken) is not null)
            return Result.Failure<Guid>(new Error("Loan.ActiveLoanExists", "La carpeta ya tiene un préstamo sin devolver."));

        var result = Loan.Create(projectId, folderId, requestedByUserId, dueAtUtc);
        if (result.IsFailure) return Result.Failure<Guid>(result.Error);

        await loans.AddAsync(result.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(result.Value.Id);
    }

    public async Task<Result> AuthorizeLoanAsync(
        Guid loanId,
        Guid authorizedByUserId,
        CancellationToken cancellationToken = default)
    {
        var loan = await loans.GetByIdAsync(loanId, cancellationToken);
        if (loan is null)
            return Result.Failure(new Error("Loan.NotFound", "No se encontró el préstamo indicado."));

        var result = loan.Authorize(authorizedByUserId);
        if (result.IsFailure) return result;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> DeliverLoanAsync(
        Guid loanId,
        Guid deliveredByUserId,
        CancellationToken cancellationToken = default)
    {
        var loan = await loans.GetByIdAsync(loanId, cancellationToken);
        if (loan is null)
            return Result.Failure(new Error("Loan.NotFound", "No se encontró el préstamo indicado."));

        var folder = await folders.GetByIdAsync(loan.FolderId, cancellationToken);
        if (folder is null)
            return Result.Failure(new Error("Loan.FolderNotFound", "No se encontró la carpeta asociada al préstamo."));
        if (folder.Status != FolderStatus.Stored || folder.LoanId is not null)
            return Result.Failure(new Error("Loan.FolderNotAvailable", "La carpeta ya no está disponible para entrega."));

        var loanResult = loan.Deliver(deliveredByUserId);
        if (loanResult.IsFailure) return loanResult;

        var folderResult = folder.StartLoan(loan.Id);
        if (folderResult.IsFailure) return folderResult;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> ReturnLoanAsync(
        Guid loanId,
        Guid returnedByUserId,
        DateTimeOffset returnedAtUtc,
        CancellationToken cancellationToken = default)
    {
        var loan = await loans.GetByIdAsync(loanId, cancellationToken);
        if (loan is null)
            return Result.Failure(new Error("Loan.NotFound", "No se encontró el préstamo indicado."));

        var folder = await folders.GetByIdAsync(loan.FolderId, cancellationToken);
        if (folder is null)
            return Result.Failure(new Error("Loan.FolderNotFound", "No se encontró la carpeta asociada al préstamo."));
        if (folder.Status != FolderStatus.Loaned || folder.LoanId != loan.Id)
            return Result.Failure(new Error("Loan.FolderLoanMismatch", "El estado y el préstamo activo de la carpeta no coinciden."));

        var loanResult = loan.Return(returnedByUserId, returnedAtUtc);
        if (loanResult.IsFailure) return loanResult;

        var folderResult = folder.ReturnLoan();
        if (folderResult.IsFailure) return folderResult;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<Guid>> RequestDigitizationAsync(
        Guid projectId,
        Guid folderId,
        Guid requestedByUserId,
        CancellationToken cancellationToken = default)
    {
        var archive = await archives.GetByProjectIdAsync(projectId, cancellationToken);
        if (archive is null)
            return Result.Failure<Guid>(new Error("Digitization.ProjectArchiveNotFound", "El proyecto no tiene un Fondo documental registrado."));

        var folder = await folders.GetByIdAsync(folderId, cancellationToken);
        if (folder is null || folder.ArchiveId != archive.Id)
            return Result.Failure<Guid>(new Error("Digitization.FolderNotInProject", "La carpeta no pertenece al proyecto indicado."));
        if (folder.Status == FolderStatus.Archived)
            return Result.Failure<Guid>(new Error("Digitization.FolderArchived", "No se puede solicitar la digitalización de una carpeta archivada de forma definitiva."));

        var result = DigitizationRequest.Create(projectId, folderId, requestedByUserId);
        if (result.IsFailure) return Result.Failure<Guid>(result.Error);

        await digitizationRequests.AddAsync(result.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(result.Value.Id);
    }

    public async Task<Result> StartDigitizationAsync(
        Guid requestId,
        Guid startedByUserId,
        CancellationToken cancellationToken = default)
    {
        var request = await digitizationRequests.GetByIdAsync(requestId, cancellationToken);
        if (request is null)
            return Result.Failure(new Error("Digitization.NotFound", "No se encontró la solicitud de digitalización indicada."));

        var result = request.Start(startedByUserId);
        if (result.IsFailure) return result;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    /// <summary>
    /// Completes a request after the caller has successfully stored and validated every binary object.
    /// This method accepts trusted application-produced metadata, not raw client-supplied storage keys.
    /// </summary>
    public async Task<Result> CompleteDigitizationAsync(
        Guid requestId,
        Guid completedByUserId,
        IReadOnlyCollection<DigitizedFileInput> storedFiles,
        CancellationToken cancellationToken = default)
    {
        var request = await digitizationRequests.GetByIdAsync(requestId, cancellationToken);
        if (request is null)
            return Result.Failure(new Error("Digitization.NotFound", "No se encontró la solicitud de digitalización indicada."));

        var result = request.Complete(completedByUserId, storedFiles);
        if (result.IsFailure) return result;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
