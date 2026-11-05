using ArchiCore.Domain.Loans;
using ArchiCore.Infrastructure.Persistence;
using ArchiCore.Infrastructure.Persistence.Loans;
using ArchiCore.Integration.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ArchiCore.Integration.Tests;

[Collection(IntegrationCollection.Name)]
public sealed class LoanOverdueProcessingTests(ApiFixture fixture) : ApiTestBase(fixture)
{
    [Fact]
    public async Task Delivered_loan_is_marked_overdue_and_emits_outbox_event_after_due_at()
    {
        var loan = Loan.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddMinutes(5));
        Assert.True(loan.IsSuccess);
        Assert.True(loan.Value.Authorize(Guid.NewGuid()).IsSuccess);
        Assert.True(loan.Value.Deliver(Guid.NewGuid()).IsSuccess);

        await Fixture.WithDbContextAsync(async db =>
        {
            db.Loans.Add(loan.Value);
            await db.SaveChangesAsync(Ct);
            return true;
        });

        var pastDue = DateTimeOffset.UtcNow.AddMinutes(-1);
        await Fixture.WithDbContextAsync(async db =>
        {
            await db.Loans.Where(item => item.Id == loan.Value.Id)
                .ExecuteUpdateAsync(update => update.SetProperty(item => item.DueAtUtc, pastDue), Ct);
            return true;
        });

        await using (var scope = Fixture.Factory.Services.CreateAsyncScope())
        {
            var processor = scope.ServiceProvider.GetRequiredService<LoanOverdueProcessor>();
            var processed = await processor.ProcessBatchAsync(DateTimeOffset.UtcNow, Ct);
            Assert.Equal(1, processed);
        }

        var persisted = await Fixture.WithDbContextAsync(db =>
            db.Loans.AsNoTracking().SingleAsync(item => item.Id == loan.Value.Id, Ct));
        Assert.Equal(LoanStatus.Overdue, persisted.Status);
        Assert.NotNull(persisted.OverdueDetectedAtUtc);

        var eventExists = await Fixture.WithDbContextAsync(db =>
            db.OutboxMessages.AnyAsync(item =>
                item.EventType.Contains("LoanOverdue") && item.Payload.Contains(loan.Value.Id.ToString()), Ct));
        Assert.True(eventExists);
    }
}
