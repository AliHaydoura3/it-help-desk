using HelpDesk.Application.Abstractions.Communication;
using HelpDesk.Application.Common.Notifications;
using HelpDesk.Domain;
using HelpDesk.Infrastructure.Email;
using HelpDesk.Infrastructure.Identity;
using HelpDesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HelpDesk.Infrastructure.Communication;

public sealed class NotificationDeliveryProcessor(
    ApplicationDbContext dbContext,
    IEmailMessageSender emailSender,
    INotificationRealtimePublisher realtimePublisher,
    IOptions<NotificationDeliveryOptions> deliveryOptions,
    IOptions<SmtpOptions> smtpOptions,
    ILogger<NotificationDeliveryProcessor> logger) : INotificationDeliveryProcessor
{
    private readonly NotificationDeliveryOptions _deliveryOptions = deliveryOptions.Value;
    private readonly SmtpOptions _smtpOptions = smtpOptions.Value;

    public async Task<int> ProcessPendingAsync(CancellationToken cancellationToken = default)
    {
        var notifications = await dbContext.Notifications
            .Where(notification =>
                (notification.RealtimeDeliveredAtUtc == null &&
                    notification.RealtimeAttempts < _deliveryOptions.MaximumAttempts) ||
                ((notification.EmailStatus == NotificationEmailStatus.Pending ||
                    notification.EmailStatus == NotificationEmailStatus.Failed) &&
                    notification.EmailAttempts < _deliveryOptions.MaximumAttempts))
            .OrderBy(notification => notification.CreatedAtUtc)
            .Take(_deliveryOptions.BatchSize)
            .ToListAsync(cancellationToken);

        if (notifications.Count == 0) return 0;

        var userIds = notifications
            .SelectMany(notification => new[]
            {
                notification.RecipientUserId,
                notification.ActorUserId
            })
            .Distinct()
            .ToArray();
        var users = await dbContext.Users.AsNoTracking()
            .Where(user => userIds.Contains(user.Id))
            .ToDictionaryAsync(user => user.Id, cancellationToken);

        foreach (var notification in notifications)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await DeliverRealtimeAsync(notification, users, cancellationToken);

            if (notification.EmailStatus is NotificationEmailStatus.Pending or NotificationEmailStatus.Failed &&
                notification.EmailAttempts < _deliveryOptions.MaximumAttempts)
            {
                await DeliverEmailAsync(notification, users, cancellationToken);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return notifications.Count;
    }

    private async Task DeliverRealtimeAsync(
        Notification notification,
        IReadOnlyDictionary<Guid, ApplicationUser> users,
        CancellationToken cancellationToken)
    {
        if (notification.RealtimeDeliveredAtUtc.HasValue ||
            notification.RealtimeAttempts >= _deliveryOptions.MaximumAttempts)
        {
            return;
        }

        try
        {
            if (!users.TryGetValue(notification.ActorUserId, out var actor))
            {
                notification.MarkRealtimeFailed("The notification actor no longer exists.");
                return;
            }

            await realtimePublisher.PublishAsync(
                new NotificationDeliveryMessage(
                    notification.Id,
                    notification.RecipientUserId,
                    new NotificationActorDelivery(
                        actor.Id,
                        actor.FirstName,
                        actor.LastName),
                    notification.TicketId,
                    notification.Type,
                    notification.Title,
                    notification.Message,
                    notification.IsRead,
                    notification.CreatedAtUtc),
                cancellationToken);
            notification.MarkRealtimeDelivered(DateTime.UtcNow);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            notification.MarkRealtimeFailed(exception.Message);
            logger.LogWarning(
                exception,
                "Realtime delivery failed for notification {NotificationId}.",
                notification.Id);
        }
    }

    private async Task DeliverEmailAsync(
        Notification notification,
        IReadOnlyDictionary<Guid, ApplicationUser> recipients,
        CancellationToken cancellationToken)
    {
        if (!emailSender.IsConfigured)
        {
            notification.SkipEmail("SMTP is not configured.");
            return;
        }

        if (!recipients.TryGetValue(notification.RecipientUserId, out var recipient) ||
            !recipient.IsActive || string.IsNullOrWhiteSpace(recipient.Email))
        {
            notification.SkipEmail("The recipient is unavailable or inactive.");
            return;
        }

        try
        {
            var ticketUrl = notification.TicketId.HasValue
                ? $"{_smtpOptions.FrontendBaseUrl.TrimEnd('/')}/tickets?ticket={notification.TicketId}"
                : _smtpOptions.FrontendBaseUrl.TrimEnd('/');
            var body = $"Hello {recipient.FirstName},\n\n{notification.Message}\n\n" +
                $"Open the help desk: {ticketUrl}\n\nIT Help Desk";

            await emailSender.SendAsync(
                recipient.Email,
                notification.Title,
                body,
                cancellationToken);
            notification.MarkEmailSent(DateTime.UtcNow);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            notification.MarkEmailFailed(exception.Message);
            logger.LogWarning(
                exception,
                "Email delivery failed for notification {NotificationId}.",
                notification.Id);
        }
    }
}
