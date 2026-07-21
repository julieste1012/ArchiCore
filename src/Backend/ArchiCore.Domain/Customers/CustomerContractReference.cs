using ArchiCore.Domain.ValueObjects;

namespace ArchiCore.Domain.Customers;

/// <summary>
/// Referencia por identidad de un contrato asociado al cliente, utilizada
/// sin acoplar el Aggregate <see cref="Customer"/> al Aggregate Contract.
/// </summary>
public sealed record CustomerContractReference(Guid ContractId, ContractNumber ContractNumber);
