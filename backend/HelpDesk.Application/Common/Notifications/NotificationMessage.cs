using HelpDesk.Domain;

namespace HelpDesk.Application.Common.Notifications;

public sealed record NotificationMessage(
    Guid ActorUserId,
    Guid? TicketId,
    NotificationType Type,
    string Title,
    string Message,
    IReadOnlyCollection<Guid> RecipientUserIds,
    bool SendEmail = true);

public sealed record NotificationDeliveryMessage(
    Guid Id,
    Guid RecipientUserId,
    NotificationActorDelivery Actor,
    Guid? TicketId,
    NotificationType Type,
    string Title,
    string Message,
    bool IsRead,
    DateTime CreatedAtUtc);

public sealed record NotificationActorDelivery(
    Guid Id,
    string FirstName,
    string LastName);
