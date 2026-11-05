using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ArchiCore.Infrastructure.Persistence.Loans;

/// <summary>
/// Evaluates delivered loans against DueAtUtc on a configurable interval.
/// No notification is emitted because the notification channel and escalation policy
/// have not yet been approved in the business-process specification.
/// </summary>
public sealed class LoanOverdueHostedService(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<LoanOverdueHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var intervalSeconds = Math.Clamp(
            configuration.GetValue<int?>("Loans:OverduePollingIntervalSeconds") ?? 300,
            30,
            3600);
        var interval = TimeSpan.FromSeconds(intervalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                int processed;
                do
                {
                    await using var scope = scopeFactory.CreateAsyncScope();
                    var processor = scope.ServiceProvider.GetRequiredService<LoanOverdueProcessor>();
                    processed = await processor.ProcessBatchAsync(cancellationToken: stoppingToken);
                }
                while (processed == LoanOverdueProcessor.BatchSize && !stoppingToken.IsCancellationRequested);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to evaluate overdue documentary loans. They will be retried.");
            }

            try
            {
                await Task.Delay(interval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }
}
