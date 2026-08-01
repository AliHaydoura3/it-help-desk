using HelpDesk.Application.Abstractions.Tickets;
using HelpDesk.Application.Features.Tickets;
using MediatR;

namespace HelpDesk.Application.Features.Tickets.ChangeTicketStatus;

public sealed class ChangeTicketStatusHandler(ITicketService ticketService)
    : IRequestHandler<ChangeTicketStatusCommand, TicketResponse>
{
    public Task<TicketResponse> Handle(ChangeTicketStatusCommand request, CancellationToken cancellationToken) =>
        ticketService.ChangeStatusAsync(request, cancellationToken);
}
