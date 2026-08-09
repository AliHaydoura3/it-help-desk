using HelpDesk.Application.Abstractions.Communication;
using MediatR;

namespace HelpDesk.Application.Features.Communication.Notifications;

public sealed class MarkNotificationReadHandler(ICommunicationService communicationService)
    : IRequestHandler<MarkNotificationReadCommand, NotificationResponse>
{
    public Task<NotificationResponse> Handle(MarkNotificationReadCommand request, CancellationToken cancellationToken) =>
        communicationService.MarkNotificationReadAsync(request, cancellationToken);
}
