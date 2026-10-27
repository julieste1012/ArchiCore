using ArchiCore.Domain.Customers;

namespace ArchiCore.Application.Customers;

public sealed record CustomerContractReferenceDto(Guid ContractId, string ContractNumber);

public sealed record CustomerDto(
    Guid Id,
    string Nit,
    CustomerStatus Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<CustomerContractReferenceDto> Contracts);

public sealed record CreateCustomerRequest(string Nit);
