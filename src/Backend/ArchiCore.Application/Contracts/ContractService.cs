using ArchiCore.Application.Abstractions.Persistence;
using ArchiCore.Application.Common;
using ArchiCore.Domain.Common;
using ArchiCore.Domain.Contracts;
using ArchiCore.Domain.Customers;
using ArchiCore.Domain.ValueObjects;

namespace ArchiCore.Application.Contracts;

public sealed class ContractService(
    IContractRepository contracts,
    ICustomerRepository customers,
    IUnitOfWork unitOfWork)
{
    public async Task<PagedResult<ContractDto>> ListAsync(
        Guid? clientId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        (page, pageSize) = NormalizePage(page, pageSize);
        var skip = (page - 1) * pageSize;
        var items = await contracts.ListAsync(clientId, skip, pageSize, cancellationToken);
        var count = await contracts.CountAsync(clientId, cancellationToken);
        return new PagedResult<ContractDto>(page, pageSize, count, items.Select(ToDto).ToArray());
    }

    public async Task<Result<ContractDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var contract = await contracts.GetByIdAsync(id, cancellationToken);
        return contract is null
            ? Result.Failure<ContractDto>(new Error("Contract.NotFound", "No se encontró el contrato solicitado."))
            : Result.Success(ToDto(contract));
    }

    public async Task<Result<ContractDto>> CreateAsync(CreateContractRequest request, CancellationToken cancellationToken = default)
    {
        var numberResult = ContractNumber.Create(request.ContractNumber);
        if (numberResult.IsFailure) return Result.Failure<ContractDto>(numberResult.Error);

        var moneyResult = Money.Create(request.TotalValue, request.Currency);
        if (moneyResult.IsFailure) return Result.Failure<ContractDto>(moneyResult.Error);

        var customer = await customers.GetByIdAsync(request.ClientId, cancellationToken);
        if (customer is null)
        {
            return Result.Failure<ContractDto>(
                new Error("Contract.CustomerNotFound", "El cliente indicado no existe."));
        }

        if (await contracts.GetByNumberAsync(numberResult.Value, cancellationToken) is not null)
        {
            return Result.Failure<ContractDto>(
                new Error("Contract.DuplicateNumber", "Ya existe un contrato con ese número."));
        }

        var createResult = Contract.Create(
            numberResult.Value,
            request.ClientId,
            request.StartDate.UtcDateTime,
            moneyResult.Value);
        if (createResult.IsFailure) return Result.Failure<ContractDto>(createResult.Error);

        var association = customer.AddContract(createResult.Value.Id, numberResult.Value);
        if (association.IsFailure) return Result.Failure<ContractDto>(association.Error);

        await contracts.AddAsync(createResult.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(ToDto(createResult.Value));
    }

    public Task<Result<ContractDto>> ActivateAsync(Guid id, CancellationToken cancellationToken = default)
        => ExecuteTransitionAsync(id, contract => contract.Activate(), cancellationToken);

    public Task<Result<ContractDto>> SuspendAsync(Guid id, CancellationToken cancellationToken = default)
        => ExecuteTransitionAsync(id, contract => contract.Suspend(), cancellationToken);

    public async Task<Result<ContractDto>> FinishAsync(Guid id, DateTimeOffset finishDate, CancellationToken cancellationToken = default)
    {
        var contract = await contracts.GetByIdAsync(id, cancellationToken);
        if (contract is null)
            return Result.Failure<ContractDto>(new Error("Contract.NotFound", "No se encontró el contrato solicitado."));

        var result = contract.Finish(finishDate.UtcDateTime);
        if (result.IsFailure) return Result.Failure<ContractDto>(result.Error);

        // Keep the owning Customer's historical reference; contract closure does not delete business history.
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(ToDto(contract));
    }

    private async Task<Result<ContractDto>> ExecuteTransitionAsync(
        Guid id, Func<Contract, Result> transition, CancellationToken cancellationToken)
    {
        var contract = await contracts.GetByIdAsync(id, cancellationToken);
        if (contract is null)
            return Result.Failure<ContractDto>(new Error("Contract.NotFound", "No se encontró el contrato solicitado."));

        var result = transition(contract);
        if (result.IsFailure) return Result.Failure<ContractDto>(result.Error);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(ToDto(contract));
    }

    private static ContractDto ToDto(Contract contract)
        => new(
            contract.Id,
            contract.ContractNumber.Value,
            contract.ClientId,
            contract.StartDate,
            contract.EndDate,
            contract.TotalValue.Amount,
            contract.TotalValue.Currency,
            contract.Status,
            contract.CreatedAt,
            contract.UpdatedAt,
            contract.Projects.Select(p => new ContractProjectReferenceDto(p.ProjectId, p.ProjectCode.Value)).ToArray());

    private static (int Page, int PageSize) NormalizePage(int page, int pageSize)
        => (Math.Max(1, page), Math.Clamp(pageSize, 1, 100));
}
