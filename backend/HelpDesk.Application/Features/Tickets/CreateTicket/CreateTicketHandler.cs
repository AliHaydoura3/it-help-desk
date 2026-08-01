using HelpDesk.Application.Abstractions.Tickets;
using HelpDesk.Application.Features.Tickets;
using MediatR;

namespace HelpDesk.Application.Features.Tickets.CreateTicket;

public sealed class CreateTicketHandler(ITicketService ticketService)
    : IRequestHandler<CreateTicketCommand, TicketResponse>
{
    public Task<TicketResponse> Handle(CreateTicketCommand request, CancellationToken cancellationToken) =>
        ticketService.CreateAsync(request, cancellationToken);
}
