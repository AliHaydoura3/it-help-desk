using HelpDesk.Application.Abstractions.Communication;
using MediatR;

namespace HelpDesk.Application.Features.Communication.Notifications;

public sealed class GetUnreadNotificationCountHandler(ICommunicationService communicationService)
    : IRequestHandler<GetUnreadNotificationCountQuery, UnreadNotificationCountResponse>
{
    public Task<UnreadNotificationCountResponse> Handle(GetUnreadNotificationCountQuery request, CancellationToken cancellationToken) =>
        communicationService.GetUnreadCountAsync(request, cancellationToken);
}
