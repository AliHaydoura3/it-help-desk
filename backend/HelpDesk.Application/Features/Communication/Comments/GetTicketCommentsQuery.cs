using HelpDesk.Application.Features.Communication;
using MediatR;

namespace HelpDesk.Application.Features.Communication.Comments;

public sealed record GetTicketCommentsQuery(
    Guid TicketId,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<GetTicketCommentsResponse>;
