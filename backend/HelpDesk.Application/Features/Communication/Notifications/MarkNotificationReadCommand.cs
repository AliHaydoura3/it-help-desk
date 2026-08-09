using HelpDesk.Application.Features.Communication;
using MediatR;

namespace HelpDesk.Application.Features.Communication.Notifications;

public sealed record MarkNotificationReadCommand(Guid NotificationId) : IRequest<NotificationResponse>;
