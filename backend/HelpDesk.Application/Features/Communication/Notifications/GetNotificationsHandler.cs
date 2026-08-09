using HelpDesk.Application.Abstractions.Communication;
using MediatR;

namespace HelpDesk.Application.Features.Communication.Notifications;

public sealed class GetNotificationsHandler(ICommunicationService communicationService)
    : IRequestHandler<GetNotificationsQuery, GetNotificationsResponse>
{
    public Task<GetNotificationsResponse> Handle(GetNotificationsQuery request, CancellationToken cancellationToken) =>
        communicationService.GetNotificationsAsync(request, cancellationToken);
}
