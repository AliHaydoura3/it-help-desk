using HelpDesk.Application.Abstractions.Tickets;
using MediatR;

namespace HelpDesk.Application.Features.Tickets.CancelTicket;

public sealed class CancelTicketHandler(ITicketService ticketService)
    : IRequestHandler<CancelTicketCommand>
{
    public Task Handle(CancelTicketCommand request, CancellationToken cancellationToken) =>
        ticketService.CancelAsync(request, cancellationToken);
}
