using HelpDesk.Application.Abstractions.Tickets;
using MediatR;

namespace HelpDesk.Application.Features.Tickets.GetTicketSummary;

public sealed class GetTicketSummaryHandler(ITicketService ticketService)
    : IRequestHandler<GetTicketSummaryQuery, TicketSummaryResponse>
{
    public Task<TicketSummaryResponse> Handle(GetTicketSummaryQuery request, CancellationToken cancellationToken) =>
        ticketService.GetSummaryAsync(request, cancellationToken);
}
