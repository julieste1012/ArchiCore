using System.Text.Json;
using ArchiCore.Domain.Customers;
using ArchiCore.Domain.Customers.Events;
using ArchiCore.Domain.ValueObjects;
using ArchiCore.Integration.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ArchiCore.Integration.Tests;

[Collection(IntegrationCollection.Name)]
public sealed class OutboxTests(ApiFixture fixture) : ApiTestBase(fixture)
{
    [Fact]
    public async Task Creating_customer_through_api_persists_its_domain_event()
    {
        var nit = TestData.Nit();
        var customer = await CreateCustomerAsync(nit);

        var messages = await Fixture.WithDbContextAsync(
            db => db.OutboxMessages.AsNoTracking()
                .Where(message => message.EventType == typeof(CustomerCreated).FullName)
                .ToListAsync(Ct));

        var matching = messages
            .Where(message => PayloadMatches(message.Payload, customer.Id, nit))
            .ToArray();

        var outboxMessage = Assert.Single(matching);
        Assert.NotEqual(Guid.Empty, outboxMessage.Id);
        Assert.Equal(typeof(CustomerCreated).FullName, outboxMessage.EventType);
        Assert.Null(outboxMessage.ProcessedAtUtc);
    }

    [Fact]
    public async Task Successful_save_clears_aggregate_events_and_a_second_save_does_not_duplicate_them()
    {
        var nit = TestData.Nit();
        var nitValue = Nit.Create(nit).Value;
        var customer = Customer.Create(nitValue).Value;
        var domainEvent = Assert.Single(customer.DomainEvents);
        var eventId = domainEvent.Id;

        var persistedEvents = await Fixture.WithDbContextAsync(async db =>
        {
            db.Customers.Add(customer);

            await db.SaveChangesAsync(Ct);
            Assert.Empty(customer.DomainEvents);

            // A later save in the same DbContext must not enqueue the already persisted event again.
            await db.SaveChangesAsync(Ct);

            return await db.OutboxMessages.AsNoTracking()
                .CountAsync(message => message.Id == eventId, Ct);
        });

        Assert.Equal(1, persistedEvents);
    }

    [Fact]
    public async Task Failed_aggregate_save_rolls_back_its_outbox_message_and_keeps_the_event_pending()
    {
        var existing = await CreateCustomerAsync();
        var duplicateNit = Nit.Create(existing.Nit).Value;
        var duplicateCustomer = Customer.Create(duplicateNit).Value;
        var eventId = Assert.Single(duplicateCustomer.DomainEvents).Id;

        var result = await Fixture.WithDbContextAsync(async db =>
        {
            db.Customers.Add(duplicateCustomer);

            await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync(Ct));

            // The failing customer insert and the event row share one transaction.
            // The event stays in memory so the caller can handle/retry the failed unit of work.
            Assert.Single(duplicateCustomer.DomainEvents);
            var outboxRowExists = await db.OutboxMessages.AsNoTracking()
                .AnyAsync(message => message.Id == eventId, Ct);
            var customerRowCount = await db.Customers.AsNoTracking()
                .CountAsync(customer => customer.Nit == duplicateNit, Ct);

            return (OutboxRowExists: outboxRowExists, CustomerRowCount: customerRowCount);
        });

        Assert.False(result.OutboxRowExists);
        Assert.Equal(1, result.CustomerRowCount);
    }

    private static bool PayloadMatches(string payload, Guid customerId, string nit)
    {
        using var document = JsonDocument.Parse(payload);
        var root = document.RootElement;

        return root.TryGetProperty("customerId", out var idElement)
            && idElement.TryGetGuid(out var parsedId)
            && parsedId == customerId
            && root.TryGetProperty("nit", out var nitElement)
            && nitElement.GetString() == nit;
    }
}
