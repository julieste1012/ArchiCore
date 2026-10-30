using ArchiCore.Domain.Common;
using ArchiCore.Domain.Customers.Events;
using ArchiCore.Domain.ValueObjects;

namespace ArchiCore.Domain.Customers;

/// <summary>
/// Aggregate Root que representa la empresa contratante que adquiere los
/// servicios ofrecidos por MULTISERTEC.
/// </summary>
public sealed class Customer : Entity, IAggregateRoot
{
    private readonly List<CustomerContractReference> _contracts = new();

    /// <summary>
    /// Número de identificación tributaria del cliente.
    /// </summary>
    public Nit Nit { get; private set; }

    /// <summary>
    /// Estado actual del cliente.
    /// </summary>
    public CustomerStatus Status { get; private set; }

    /// <summary>
    /// Contratos asociados al cliente, referenciados por identidad.
    /// </summary>
    public IReadOnlyCollection<CustomerContractReference> Contracts => _contracts.AsReadOnly();

    private Customer()
    {
        Nit = null!;
    }

    private Customer(Nit nit)
    {
        Nit = nit;
        Status = CustomerStatus.Prospect;
    }

    /// <summary>
    /// Crea un nuevo cliente en estado <see cref="CustomerStatus.Prospect"/>.
    /// </summary>
    public static Result<Customer> Create(Nit nit)
    {
        if (nit is null)
        {
            return Result.Failure<Customer>(
                new Error("Customer.NitRequired", "El NIT del cliente es obligatorio."));
        }

        var customer = new Customer(nit);

        customer.AddDomainEvent(new CustomerCreated(customer.Id, customer.Nit.Value));

        return Result.Success(customer);
    }

    /// <summary>
    /// Activa el cliente desde el estado <see cref="CustomerStatus.Prospect"/>.
    /// </summary>
    public Result Activate()
    {
        if (Status != CustomerStatus.Prospect)
        {
            return Result.Failure(
                new Error("Customer.InvalidActivation", "El cliente solo puede activarse desde el estado Prospect."));
        }

        Status = CustomerStatus.Active;
        MarkAsUpdated();
        AddDomainEvent(new CustomerActivated(Id));

        return Result.Success();
    }

    /// <summary>
    /// Desactiva el cliente desde el estado <see cref="CustomerStatus.Active"/>.
    /// </summary>
    public Result Deactivate()
    {
        if (Status != CustomerStatus.Active)
        {
            return Result.Failure(
                new Error("Customer.InvalidDeactivation", "El cliente solo puede desactivarse desde el estado Active."));
        }

        Status = CustomerStatus.Inactive;
        MarkAsUpdated();
        AddDomainEvent(new CustomerDeactivated(Id));

        return Result.Success();
    }

    /// <summary>
    /// Archiva el cliente desde el estado <see cref="CustomerStatus.Inactive"/>.
    /// </summary>
    public Result Archive()
    {
        if (Status != CustomerStatus.Inactive)
        {
            return Result.Failure(
                new Error("Customer.InvalidArchival", "El cliente solo puede archivarse desde el estado Inactive."));
        }

        Status = CustomerStatus.Archived;
        MarkAsUpdated();
        AddDomainEvent(new CustomerArchived(Id));

        return Result.Success();
    }

    /// <summary>
    /// Asocia un contrato al cliente.
    /// </summary>
    public Result AddContract(Guid contractId, ContractNumber contractNumber)
    {
        if (contractId == Guid.Empty)
        {
            return Result.Failure(
                new Error("Customer.ContractIdRequired", "El identificador del contrato es obligatorio."));
        }

        if (contractNumber is null)
        {
            return Result.Failure(
                new Error("Customer.ContractNumberRequired", "El número de contrato es obligatorio."));
        }

        if (Status == CustomerStatus.Archived)
        {
            return Result.Failure(
                new Error("Customer.ArchivedCannotAddContract", "Un cliente archivado no podrá generar nuevos contratos."));
        }

        if (ContainsContract(contractNumber))
        {
            return Result.Failure(
                new Error("Customer.DuplicateContract", "El contrato ya se encuentra asociado al cliente."));
        }

        _contracts.Add(new CustomerContractReference(contractId, contractNumber));
        MarkAsUpdated();

        return Result.Success();
    }

    /// <summary>
    /// Elimina un contrato previamente asociado al cliente.
    /// </summary>
    public Result RemoveContract(Guid contractId)
    {
        var contract = _contracts.FirstOrDefault(c => c.ContractId == contractId);

        if (contract is null)
        {
            return Result.Failure(
                new Error("Customer.ContractNotFound", "El contrato especificado no pertenece a este cliente."));
        }

        _contracts.Remove(contract);
        MarkAsUpdated();

        return Result.Success();
    }

    private bool ContainsContract(ContractNumber contractNumber)
    {
        return _contracts.Any(c => c.ContractNumber == contractNumber);
    }
}
