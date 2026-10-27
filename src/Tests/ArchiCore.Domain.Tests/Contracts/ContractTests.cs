using ArchiCore.Domain.Contracts;
using ArchiCore.Domain.Contracts.Events;
using ArchiCore.Domain.ValueObjects;
using Xunit;

namespace ArchiCore.Domain.Tests.Contracts;

public sealed class ContractTests
{
    private static Contract CreateContract(DateTime? startDate = null)
    {
        return Contract.Create(
            ContractNumber.Create("CTR-2026-000001").Value,
            Guid.NewGuid(),
            startDate ?? new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Money.Create(120000m, "COP").Value).Value;
    }

    [Fact]
    public void Create_requires_a_client_id()
    {
        var result = Contract.Create(
            ContractNumber.Create("CTR-2026-000001").Value,
            Guid.Empty,
            DateTime.UtcNow,
            Money.Create(100m, "COP").Value);

        Assert.True(result.IsFailure);
        Assert.Equal("Contract.ClientRequired", result.Error.Code);
    }

    [Fact]
    public void Create_starts_as_draft_and_emits_contract_created()
    {
        var contract = CreateContract();

        Assert.Equal(ContractStatus.Draft, contract.Status);
        Assert.IsType<ContractCreated>(Assert.Single(contract.DomainEvents));
    }

    [Fact]
    public void Contract_lifecycle_supports_activate_suspend_reactivate_and_finish()
    {
        var contract = CreateContract();

        Assert.True(contract.Activate().IsSuccess);
        Assert.Equal(ContractStatus.Active, contract.Status);

        Assert.True(contract.Suspend().IsSuccess);
        Assert.Equal(ContractStatus.Suspended, contract.Status);

        Assert.True(contract.Activate().IsSuccess);
        var finishDate = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc);
        Assert.True(contract.Finish(finishDate).IsSuccess);
        Assert.Equal(ContractStatus.Finished, contract.Status);
        Assert.Equal(finishDate, contract.EndDate);
    }

    [Fact]
    public void Contract_cannot_finish_before_its_start_date()
    {
        var contract = CreateContract(new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc));
        Assert.True(contract.Activate().IsSuccess);

        var result = contract.Finish(new DateTime(2026, 1, 9, 0, 0, 0, DateTimeKind.Utc));

        Assert.True(result.IsFailure);
        Assert.Equal("Contract.FinishBeforeStart", result.Error.Code);
        Assert.Equal(ContractStatus.Active, contract.Status);
        Assert.Null(contract.EndDate);
    }

    [Fact]
    public void Project_code_cannot_be_added_twice_to_the_same_contract()
    {
        var contract = CreateContract();
        var codeA = ProjectCode.Create("PRJ-2026-000001").Value;
        var codeB = ProjectCode.Create("PRJ-2026-000001").Value;

        Assert.True(contract.AddProject(Guid.NewGuid(), codeA).IsSuccess);
        var duplicate = contract.AddProject(Guid.NewGuid(), codeB);

        Assert.True(duplicate.IsFailure);
        Assert.Equal("Contract.DuplicateProjectCode", duplicate.Error.Code);
        Assert.Single(contract.Projects);
    }
}
