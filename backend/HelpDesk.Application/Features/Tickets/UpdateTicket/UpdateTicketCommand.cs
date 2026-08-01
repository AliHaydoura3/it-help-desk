using HelpDesk.Domain;
using HelpDesk.Application.Features.Tickets;
using MediatR;

namespace HelpDesk.Application.Features.Tickets.UpdateTicket;

public sealed record UpdateTicketCommand(
    string Title,
    string Description,
    TicketCategory Category,
    TicketPriority Priority) : IRequest<TicketResponse>
{
    public Guid Id { get; init; }
}
