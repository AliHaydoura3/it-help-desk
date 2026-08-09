using HelpDesk.Application.Common.Notifications;

namespace HelpDesk.Application.Abstractions.Communication;

public interface INotificationQueue
{
    Task QueueAsync(NotificationMessage message, CancellationToken cancellationToken);

    Task QueueToRolesAsync(
        NotificationMessage message,
        IReadOnlyCollection<string> roleNames,
        CancellationToken cancellationToken);
}
