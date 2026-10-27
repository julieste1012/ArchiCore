using ArchiCore.Domain.Customers;
using ArchiCore.Domain.Customers.Events;
using ArchiCore.Domain.ValueObjects;
using Xunit;

namespace ArchiCore.Domain.Tests.Customers;

public sealed class CustomerTests
{
    [Fact]
    public void Create_requires_a_nit()
    {
        var result = Customer.Create(null!);

        Assert.True(result.IsFailure);
        Assert.Equal("Customer.NitRequired", result.Error.Code);
    }

    [Fact]
    public void Create_starts_as_prospect_and_emits_customer_created()
    {
        var customer = Customer.Create(Nit.Create("900123456-1").Value).Value;

        Assert.Equal(CustomerStatus.Prospect, customer.Status);
        Assert.IsType<CustomerCreated>(Assert.Single(customer.DomainEvents));
    }

    [Fact]
    public void Customer_can_be_activated_only_from_prospect()
    {
        var customer = Customer.Create(Nit.Create("900123456-1").Value).Value;

        var firstActivation = customer.Activate();
        var secondActivation = customer.Activate();

        Assert.True(firstActivation.IsSuccess);
        Assert.True(secondActivation.IsFailure);
        Assert.Equal(CustomerStatus.Active, customer.Status);
        Assert.Equal("Customer.InvalidActivation", secondActivation.Error.Code);
    }

    [Fact]
    public void Duplicate_contract_number_cannot_be_added_twice()
    {
        var customer = Customer.Create(Nit.Create("900123456-1").Value).Value;
        var numberA = ContractNumber.Create("CTR-2026-000001").Value;
        var numberB = ContractNumber.Create("CTR-2026-000001").Value;

        Assert.True(customer.AddContract(Guid.NewGuid(), numberA).IsSuccess);
        var duplicate = customer.AddContract(Guid.NewGuid(), numberB);

        Assert.True(duplicate.IsFailure);
        Assert.Equal("Customer.DuplicateContract", duplicate.Error.Code);
        Assert.Single(customer.Contracts);
    }

    [Fact]
    public void Archived_customer_cannot_receive_new_contracts()
    {
        var customer = Customer.Create(Nit.Create("900123456-1").Value).Value;

        Assert.True(customer.Activate().IsSuccess);
        Assert.True(customer.Deactivate().IsSuccess);
        Assert.True(customer.Archive().IsSuccess);

        var result = customer.AddContract(
            Guid.NewGuid(),
            ContractNumber.Create("CTR-2026-000001").Value);

        Assert.True(result.IsFailure);
        Assert.Equal("Customer.ArchivedCannotAddContract", result.Error.Code);
    }
}
