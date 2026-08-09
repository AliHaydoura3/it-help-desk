using HelpDesk.Application.Features.Communication;
using MediatR;

namespace HelpDesk.Application.Features.Communication.Notifications;

public sealed record GetNotificationsQuery(
    bool? IsRead = null,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<GetNotificationsResponse>;
