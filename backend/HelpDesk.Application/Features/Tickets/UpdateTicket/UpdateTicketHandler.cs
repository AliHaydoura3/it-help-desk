using HelpDesk.Application.Abstractions.Tickets;
using HelpDesk.Application.Features.Tickets;
using MediatR;

namespace HelpDesk.Application.Features.Tickets.UpdateTicket;

public sealed class UpdateTicketHandler(ITicketService ticketService)
    : IRequestHandler<UpdateTicketCommand, TicketResponse>
{
    public Task<TicketResponse> Handle(UpdateTicketCommand request, CancellationToken cancellationToken) =>
        ticketService.UpdateAsync(request, cancellationToken);
}
