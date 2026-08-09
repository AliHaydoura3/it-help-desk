using HelpDesk.Application.Abstractions.Communication;
using HelpDesk.Application.Common.Notifications;
using Microsoft.AspNetCore.SignalR;

namespace HelpDesk.Api.Notifications;

public sealed class SignalRNotificationRealtimePublisher(
    IHubContext<NotificationHub> hubContext) : INotificationRealtimePublisher
{
    public Task PublishAsync(
        NotificationDeliveryMessage notification,
        CancellationToken cancellationToken = default) =>
        hubContext.Clients
            .Group(NotificationHub.GetUserGroup(notification.RecipientUserId.ToString()))
            .SendAsync("notificationReceived", notification, cancellationToken);
}
