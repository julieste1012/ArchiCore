using ArchiCore.Domain.Loans;
using ArchiCore.Domain.Loans.Events;
using Xunit;

namespace ArchiCore.Domain.Tests.Loans;

public sealed class LoanTests
{
    private static Loan CreateLoan()
        => Loan.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(7)).Value;

    [Fact]
    public void Create_requires_project_folder_requester_and_future_due_date()
    {
        var invalid = Loan.Create(Guid.Empty, Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(1));
        Assert.True(invalid.IsFailure);
        Assert.Equal("Loan.ProjectRequired", invalid.Error.Code);

        var dueDate = DateTimeOffset.UtcNow.AddDays(-1);
        var invalidDueDate = Loan.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), dueDate);
        Assert.True(invalidDueDate.IsFailure);
        Assert.Equal("Loan.InvalidDueDate", invalidDueDate.Error.Code);
    }

    [Fact]
    public void Loan_lifecycle_records_request_authorization_delivery_and_return()
    {
        var loan = CreateLoan();
        Assert.Equal(LoanStatus.Requested, loan.Status);
        Assert.IsType<LoanRequested>(Assert.Single(loan.DomainEvents));

        Assert.True(loan.Authorize(Guid.NewGuid()).IsSuccess);
        Assert.Equal(LoanStatus.Authorized, loan.Status);

        Assert.True(loan.Deliver(Guid.NewGuid()).IsSuccess);
        Assert.Equal(LoanStatus.Delivered, loan.Status);

        var returnedAt = loan.DeliveredAtUtc!.Value.AddHours(1);
        Assert.True(loan.Return(Guid.NewGuid(), returnedAt).IsSuccess);
        Assert.Equal(LoanStatus.Returned, loan.Status);
        Assert.Equal(returnedAt, loan.ReturnedAtUtc);
        Assert.Contains(loan.DomainEvents, item => item is LoanAuthorized);
        Assert.Contains(loan.DomainEvents, item => item is LoanDelivered);
        Assert.Contains(loan.DomainEvents, item => item is LoanReturned);
    }

    [Fact]
    public void Loan_cannot_be_delivered_before_authorization()
    {
        var loan = CreateLoan();
        var result = loan.Deliver(Guid.NewGuid());

        Assert.True(result.IsFailure);
        Assert.Equal("Loan.InvalidDelivery", result.Error.Code);
        Assert.Equal(LoanStatus.Requested, loan.Status);
    }

    [Fact]
    public void Loan_becomes_overdue_only_after_due_date_and_can_still_be_returned()
    {
        var loan = CreateLoan();
        Assert.True(loan.Authorize(Guid.NewGuid()).IsSuccess);
        Assert.True(loan.Deliver(Guid.NewGuid()).IsSuccess);

        var notYetDue = loan.MarkOverdue(loan.DueAtUtc);
        Assert.True(notYetDue.IsFailure);
        Assert.Equal("Loan.NotYetOverdue", notYetDue.Error.Code);

        var overdueAt = loan.DueAtUtc.AddMinutes(1);
        Assert.True(loan.MarkOverdue(overdueAt).IsSuccess);
        Assert.Equal(LoanStatus.Overdue, loan.Status);
        Assert.IsType<LoanOverdue>(loan.DomainEvents.Last());

        Assert.True(loan.Return(Guid.NewGuid(), overdueAt.AddHours(1)).IsSuccess);
        Assert.Equal(LoanStatus.Returned, loan.Status);
    }

    [Fact]
    public void Loan_cannot_be_returned_before_delivery_or_twice()
    {
        var loan = CreateLoan();
        var early = loan.Return(Guid.NewGuid(), DateTimeOffset.UtcNow);
        Assert.True(early.IsFailure);

        Assert.True(loan.Authorize(Guid.NewGuid()).IsSuccess);
        Assert.True(loan.Deliver(Guid.NewGuid()).IsSuccess);
        Assert.True(loan.Return(Guid.NewGuid(), loan.DeliveredAtUtc!.Value).IsSuccess);

        var duplicate = loan.Return(Guid.NewGuid(), DateTimeOffset.UtcNow);
        Assert.True(duplicate.IsFailure);
        Assert.Equal(LoanStatus.Returned, loan.Status);
    }
}
