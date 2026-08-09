using HelpDesk.Application.Common.Notifications;

namespace HelpDesk.Application.Abstractions.Communication;

public interface INotificationRealtimePublisher
{
    Task PublishAsync(
        NotificationDeliveryMessage notification,
        CancellationToken cancellationToken = default);
}
