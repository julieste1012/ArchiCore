using ArchiCore.Application.Abstractions.Persistence;
using ArchiCore.Application.Common;
using ArchiCore.Domain.Common;
using ArchiCore.Domain.Customers;
using ArchiCore.Domain.ValueObjects;

namespace ArchiCore.Application.Customers;

public sealed class CustomerService(
    ICustomerRepository customers,
    IUnitOfWork unitOfWork)
{
    public async Task<PagedResult<CustomerDto>> ListAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        (page, pageSize) = NormalizePage(page, pageSize);
        var skip = (page - 1) * pageSize;
        var items = await customers.ListAsync(skip, pageSize, cancellationToken);
        var count = await customers.CountAsync(cancellationToken);
        return new PagedResult<CustomerDto>(page, pageSize, count, items.Select(ToDto).ToArray());
    }

    public async Task<Result<CustomerDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await customers.GetByIdAsync(id, cancellationToken);
        return customer is null
            ? Result.Failure<CustomerDto>(new Error("Customer.NotFound", "No se encontró el cliente solicitado."))
            : Result.Success(ToDto(customer));
    }

    public async Task<Result<CustomerDto>> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        var nitResult = Nit.Create(request.Nit);
        if (nitResult.IsFailure)
        {
            return Result.Failure<CustomerDto>(nitResult.Error);
        }

        if (await customers.GetByNitAsync(nitResult.Value, cancellationToken) is not null)
        {
            return Result.Failure<CustomerDto>(
                new Error("Customer.DuplicateNit", "Ya existe un cliente registrado con ese NIT."));
        }

        var createResult = Customer.Create(nitResult.Value);
        if (createResult.IsFailure)
        {
            return Result.Failure<CustomerDto>(createResult.Error);
        }

        await customers.AddAsync(createResult.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(ToDto(createResult.Value));
    }

    public Task<Result<CustomerDto>> ActivateAsync(Guid id, CancellationToken cancellationToken = default)
        => ExecuteTransitionAsync(id, (customer) => customer.Activate(), cancellationToken);

    public Task<Result<CustomerDto>> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
        => ExecuteTransitionAsync(id, (customer) => customer.Deactivate(), cancellationToken);

    public Task<Result<CustomerDto>> ArchiveAsync(Guid id, CancellationToken cancellationToken = default)
        => ExecuteTransitionAsync(id, (customer) => customer.Archive(), cancellationToken);

    private async Task<Result<CustomerDto>> ExecuteTransitionAsync(
        Guid id, Func<Customer, Result> transition, CancellationToken cancellationToken)
    {
        var customer = await customers.GetByIdAsync(id, cancellationToken);
        if (customer is null)
        {
            return Result.Failure<CustomerDto>(
                new Error("Customer.NotFound", "No se encontró el cliente solicitado."));
        }

        var result = transition(customer);
        if (result.IsFailure)
        {
            return Result.Failure<CustomerDto>(result.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(ToDto(customer));
    }

    private static CustomerDto ToDto(Customer customer)
        => new(
            customer.Id,
            customer.Nit.Value,
            customer.Status,
            customer.CreatedAt,
            customer.UpdatedAt,
            customer.Contracts.Select(c => new CustomerContractReferenceDto(c.ContractId, c.ContractNumber.Value)).ToArray());

    private static (int Page, int PageSize) NormalizePage(int page, int pageSize)
        => (Math.Max(1, page), Math.Clamp(pageSize, 1, 100));
}
