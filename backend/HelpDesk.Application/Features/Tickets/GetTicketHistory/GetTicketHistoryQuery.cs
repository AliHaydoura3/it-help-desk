using MediatR;
using HelpDesk.Application.Features.Tickets;

namespace HelpDesk.Application.Features.Tickets.GetTicketHistory;

public sealed record GetTicketHistoryQuery(Guid Id)
    : IRequest<IReadOnlyList<TicketHistoryResponse>>;
