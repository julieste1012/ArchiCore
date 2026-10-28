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
