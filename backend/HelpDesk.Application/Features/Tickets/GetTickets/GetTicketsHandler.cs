using HelpDesk.Application.Abstractions.Tickets;
using MediatR;

namespace HelpDesk.Application.Features.Tickets.GetTickets;

public sealed class GetTicketsHandler(ITicketService ticketService)
    : IRequestHandler<GetTicketsQuery, GetTicketsResponse>
{
    public Task<GetTicketsResponse> Handle(GetTicketsQuery request, CancellationToken cancellationToken) =>
        ticketService.GetAllAsync(request, cancellationToken);
}
