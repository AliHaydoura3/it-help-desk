using HelpDesk.Application.Abstractions.Communication;
using HelpDesk.Infrastructure.Communication;
using Microsoft.Extensions.Options;

namespace HelpDesk.Api.Notifications;

public sealed class NotificationDeliveryWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<NotificationDeliveryOptions> options,
    ILogger<NotificationDeliveryWorker> logger) : BackgroundService
{
    private readonly TimeSpan _pollInterval =
        TimeSpan.FromSeconds(options.Value.PollIntervalSeconds);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var processor = scope.ServiceProvider
                    .GetRequiredService<INotificationDeliveryProcessor>();
                await processor.ProcessPendingAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Notification delivery cycle failed.");
            }

            await Task.Delay(_pollInterval, stoppingToken);
        }
    }
}
