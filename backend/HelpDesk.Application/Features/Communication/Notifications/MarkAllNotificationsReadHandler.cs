using HelpDesk.Application.Abstractions.Communication;
using MediatR;

namespace HelpDesk.Application.Features.Communication.Notifications;

public sealed class MarkAllNotificationsReadHandler(ICommunicationService communicationService)
    : IRequestHandler<MarkAllNotificationsReadCommand, MarkAllNotificationsReadResponse>
{
    public Task<MarkAllNotificationsReadResponse> Handle(MarkAllNotificationsReadCommand request, CancellationToken cancellationToken) =>
        communicationService.MarkAllNotificationsReadAsync(request, cancellationToken);
}
