using System.Net;
using ArchiCore.Domain.Customers.Events;
using ArchiCore.Integration.Tests.Infrastructure;
using ArchiCore.Infrastructure.Persistence.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ArchiCore.Integration.Tests;

[Collection(IntegrationCollection.Name)]
public sealed class DomainEventAuditApiTests(ApiFixture fixture) : ApiTestBase(fixture)
{
    [Fact]
    public async Task Administrator_can_filter_and_page_the_durable_domain_event_audit()
    {
        var customer = await CreateCustomerAsync();
        var eventType = typeof(CustomerCreated).FullName!;

        var outboxId = await Fixture.WithDbContextAsync(async db =>
            (await db.OutboxMessages.AsNoTracking()
                .Where(message => message.EventType == eventType)
                .OrderByDescending(message => message.EnqueuedAtUtc)
                .FirstAsync(Ct)).Id);

        using (var scope = Fixture.Factory.Services.CreateScope())
        {
            var processor = scope.ServiceProvider.GetRequiredService<IDomainEventOutboxProcessor>();
            await processor.ProcessBatchAsync(Ct);
        }

        var query = $"/api/audit/domain-events?eventType={Uri.EscapeDataString(eventType)}&page=1&pageSize=100";
        var audit = await GetJsonAsync<PagedResponse<DomainEventAuditResponse>>(query);

        Assert.True(audit.TotalCount > 0);
        var record = Assert.Single(audit.Items, item => item.Id == outboxId);
        Assert.Equal(eventType, record.EventType);
        Assert.True(record.RecordedAtUtc <= DateTime.UtcNow);
        Assert.Contains(customer.Id.ToString(), record.Payload);
    }

    [Fact]
    public async Task Invalid_audit_date_range_returns_422()
    {
        using var response = await GetAsync(
            "/api/audit/domain-events?fromUtc=2026-12-31T00%3A00%3A00Z&toUtc=2026-01-01T00%3A00%3A00Z");

        await AssertStatusAsync(HttpStatusCode.UnprocessableEntity, response);
        var problem = await ReadBodyAsync<ProblemResponse>(response);
        Assert.Equal("Audit.InvalidDateRange", problem.Code);
    }
}
