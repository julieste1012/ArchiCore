using System.Text.Json;
using ArchiCore.Domain.Customers.Events;
using ArchiCore.Integration.Tests.Infrastructure;
using ArchiCore.Infrastructure.Persistence.Audit;
using ArchiCore.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ArchiCore.Integration.Tests;

[Collection(IntegrationCollection.Name)]
public sealed class OutboxDispatchTests(ApiFixture fixture) : ApiTestBase(fixture)
{
    [Fact]
    public async Task Processor_projects_domain_event_to_audit_and_marks_outbox_atomically()
    {
        var customer = await CreateCustomerAsync();
        var outboxMessage = await Fixture.WithDbContextAsync(db =>
            db.OutboxMessages.AsNoTracking()
                .Where(message => message.EventType == typeof(CustomerCreated).FullName)
                .OrderByDescending(message => message.EnqueuedAtUtc)
                .FirstAsync(Ct));

        using var scope = Fixture.Factory.Services.CreateScope();
        var processor = scope.ServiceProvider.GetRequiredService<IDomainEventOutboxProcessor>();

        var processedCount = await processor.ProcessBatchAsync(Ct);

        Assert.True(processedCount >= 1);

        var persisted = await Fixture.WithDbContextAsync(async db =>
        {
            var message = await db.OutboxMessages.AsNoTracking()
                .SingleAsync(candidate => candidate.Id == outboxMessage.Id, Ct);
            var audit = await db.DomainEventAuditEntries.AsNoTracking()
                .SingleAsync(candidate => candidate.Id == outboxMessage.Id, Ct);
            return (Message: message, Audit: audit);
        });

        Assert.NotNull(persisted.Message.ProcessedAtUtc);
        Assert.Equal(outboxMessage.EventType, persisted.Audit.EventType);
        Assert.Equal(outboxMessage.Payload, persisted.Audit.Payload);
        Assert.Equal(outboxMessage.OccurredOnUtc, persisted.Audit.OccurredOnUtc);

        using var payload = JsonDocument.Parse(persisted.Audit.Payload);
        Assert.Equal(customer.Id, payload.RootElement.GetProperty("customerId").GetGuid());

        var processedAgain = await processor.ProcessBatchAsync(Ct);
        Assert.Equal(0, processedAgain);

        var auditRows = await Fixture.WithDbContextAsync(db =>
            db.DomainEventAuditEntries.AsNoTracking().CountAsync(entry => entry.Id == outboxMessage.Id, Ct));
        Assert.Equal(1, auditRows);
    }
}
