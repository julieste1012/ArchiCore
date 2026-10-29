using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ArchiCore.Infrastructure.Persistence.Outbox;

/// <summary>
/// Polls the outbox for pending domain events. Disabled in the Testing environment so
/// integration tests can exercise the processor deterministically.
/// </summary>
public sealed class DomainEventOutboxHostedService(
    IServiceScopeFactory scopeFactory,
    ILogger<DomainEventOutboxHostedService> logger) : BackgroundService
{
    private static readonly TimeSpan IdleDelay = TimeSpan.FromSeconds(3);
    private static readonly TimeSpan FailureDelay = TimeSpan.FromSeconds(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                int processed;
                await using (var scope = scopeFactory.CreateAsyncScope())
                {
                    var processor = scope.ServiceProvider.GetRequiredService<IDomainEventOutboxProcessor>();
                    processed = await processor.ProcessBatchAsync(stoppingToken);
                }

                if (processed == 0)
                {
                    await Task.Delay(IdleDelay, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to process the domain-event outbox. Pending rows will be retried.");
                await Task.Delay(FailureDelay, stoppingToken);
            }
        }
    }
}
