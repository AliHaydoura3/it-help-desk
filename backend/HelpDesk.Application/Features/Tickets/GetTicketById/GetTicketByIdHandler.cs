using HelpDesk.Application.Abstractions.Tickets;
using HelpDesk.Application.Features.Tickets;
using MediatR;

namespace HelpDesk.Application.Features.Tickets.GetTicketById;

public sealed class GetTicketByIdHandler(ITicketService ticketService)
    : IRequestHandler<GetTicketByIdQuery, TicketResponse>
{
    public Task<TicketResponse> Handle(GetTicketByIdQuery request, CancellationToken cancellationToken) =>
        ticketService.GetByIdAsync(request, cancellationToken);
}
