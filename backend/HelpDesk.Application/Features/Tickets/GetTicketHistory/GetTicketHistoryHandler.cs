using HelpDesk.Application.Abstractions.Tickets;
using HelpDesk.Application.Features.Tickets;
using MediatR;

namespace HelpDesk.Application.Features.Tickets.GetTicketHistory;

public sealed class GetTicketHistoryHandler(ITicketService ticketService)
    : IRequestHandler<GetTicketHistoryQuery, IReadOnlyList<TicketHistoryResponse>>
{
    public Task<IReadOnlyList<TicketHistoryResponse>> Handle(GetTicketHistoryQuery request, CancellationToken cancellationToken) =>
        ticketService.GetHistoryAsync(request, cancellationToken);
}
