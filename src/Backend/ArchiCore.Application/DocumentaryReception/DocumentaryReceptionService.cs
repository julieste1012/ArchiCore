using ArchiCore.Application.Abstractions.Persistence;
using ArchiCore.Application.Common;
using ArchiCore.Domain.Common;
using ArchiCore.Domain.Contracts;
using ArchiCore.Domain.DocumentaryReception;

namespace ArchiCore.Application.Receptions;

public sealed class DocumentaryReceptionService(
    IDocumentaryReceptionRepository receptions,
    IContractRepository contracts,
    ICustomerRepository customers,
    IUnitOfWork unitOfWork)
{
    public async Task<PagedResult<DocumentaryReceptionDto>> ListAsync(
        Guid? contractId,
        DocumentaryReceptionStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        (page, pageSize) = NormalizePage(page, pageSize);
        var skip = (page - 1) * pageSize;
        var items = await receptions.ListAsync(contractId, status, skip, pageSize, cancellationToken);
        var totalCount = await receptions.CountAsync(contractId, status, cancellationToken);
        return new PagedResult<DocumentaryReceptionDto>(
            page, pageSize, totalCount, items.Select(ToDto).ToArray());
    }

    public async Task<Result<DocumentaryReceptionDto>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var reception = await receptions.GetByIdAsync(id, cancellationToken);
        return reception is null
            ? Result.Failure<DocumentaryReceptionDto>(
                new Error("Reception.NotFound", "The documentary reception was not found."))
            : Result.Success(ToDto(reception));
    }

    public async Task<Result<DocumentaryReceptionDto>> CreateAsync(
        CreateDocumentaryReceptionRequest request,
        Guid receivedByUserId,
        CancellationToken cancellationToken = default)
    {
        var contract = await contracts.GetByIdAsync(request.ContractId, cancellationToken);
        if (contract is null)
            return Result.Failure<DocumentaryReceptionDto>(
                new Error("Reception.ContractNotFound", "The contract associated with the delivery was not found."));

        if (contract.Status != ContractStatus.Active)
            return Result.Failure<DocumentaryReceptionDto>(
                new Error("Reception.ContractNotActive", "Documentary reception requires an active contract."));

        var customer = await customers.GetByIdAsync(contract.ClientId, cancellationToken);
        if (customer is null)
            return Result.Failure<DocumentaryReceptionDto>(
                new Error("Reception.CustomerNotFound", "The customer associated with the contract was not found."));

        var creation = DocumentaryReception.Create(
            contract.Id, customer.Id, receivedByUserId, request.InitialObservations);
        if (creation.IsFailure)
            return Result.Failure<DocumentaryReceptionDto>(creation.Error);

        await receptions.AddAsync(creation.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(ToDto(creation.Value));
    }

    public async Task<Result<DocumentaryReceptionDto>> RecordInspectionAsync(
        Guid id,
        RecordReceptionInspectionRequest request,
        Guid inspectedByUserId,
        CancellationToken cancellationToken = default)
    {
        var reception = await receptions.GetByIdAsync(id, cancellationToken);
        if (reception is null)
            return Result.Failure<DocumentaryReceptionDto>(
                new Error("Reception.NotFound", "The documentary reception was not found."));

        var result = reception.RecordInspection(inspectedByUserId, request.SafeToAccept, request.Observations);
        if (result.IsFailure)
            return Result.Failure<DocumentaryReceptionDto>(result.Error);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(ToDto(reception));
    }

    public async Task<Result<DocumentaryReceptionDto>> RecordCountingAsync(
        Guid id,
        RecordReceptionCountingRequest request,
        Guid countedByUserId,
        CancellationToken cancellationToken = default)
    {
        var reception = await receptions.GetByIdAsync(id, cancellationToken);
        if (reception is null)
            return Result.Failure<DocumentaryReceptionDto>(
                new Error("Reception.NotFound", "The documentary reception was not found."));

        var result = reception.RecordCounting(
            countedByUserId,
            request.DeclaredBoxCount,
            request.DeclaredFolderCount,
            request.CountedBoxCount,
            request.CountedFolderCount,
            request.DiscrepancyReport);
        if (result.IsFailure)
            return Result.Failure<DocumentaryReceptionDto>(result.Error);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(ToDto(reception));
    }

    public async Task<Result<DocumentaryReceptionDto>> ApproveCountingAsync(
        Guid id,
        Guid approvedByUserId,
        CancellationToken cancellationToken = default)
    {
        var reception = await receptions.GetByIdAsync(id, cancellationToken);
        if (reception is null)
            return Result.Failure<DocumentaryReceptionDto>(
                new Error("Reception.NotFound", "The documentary reception was not found."));

        var result = reception.ApproveCounting(approvedByUserId);
        if (result.IsFailure)
            return Result.Failure<DocumentaryReceptionDto>(result.Error);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(ToDto(reception));
    }

    public async Task<Result<DocumentaryReceptionDto>> SuspendAsync(
        Guid id,
        string reason,
        Guid suspendedByUserId,
        CancellationToken cancellationToken = default)
    {
        var reception = await receptions.GetByIdAsync(id, cancellationToken);
        if (reception is null)
            return Result.Failure<DocumentaryReceptionDto>(
                new Error("Reception.NotFound", "The documentary reception was not found."));

        var result = reception.Suspend(suspendedByUserId, reason);
        if (result.IsFailure)
            return Result.Failure<DocumentaryReceptionDto>(result.Error);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(ToDto(reception));
    }

    private static DocumentaryReceptionDto ToDto(DocumentaryReception reception)
        => new(
            reception.Id,
            reception.ContractId,
            reception.CustomerId,
            reception.ReceivedByUserId,
            reception.ReceivedAtUtc,
            reception.InitialObservations,
            reception.Status,
            reception.InspectedByUserId,
            reception.InspectionCompletedAtUtc,
            reception.SafeToAccept,
            reception.InspectionObservations,
            reception.DeclaredBoxCount,
            reception.DeclaredFolderCount,
            reception.CountedBoxCount,
            reception.CountedFolderCount,
            reception.DiscrepancyReport,
            reception.CountedByUserId,
            reception.CountedAtUtc,
            reception.ApprovedByUserId,
            reception.ApprovedAtUtc,
            reception.SuspendedByUserId,
            reception.SuspendedAtUtc,
            reception.SuspensionReason,
            reception.CreatedAt,
            reception.UpdatedAt);

    private static (int Page, int PageSize) NormalizePage(int page, int pageSize)
        => (Math.Max(1, page), Math.Clamp(pageSize, 1, 100));
}
