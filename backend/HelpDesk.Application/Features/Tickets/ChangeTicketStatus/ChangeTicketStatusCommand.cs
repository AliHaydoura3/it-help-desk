using HelpDesk.Domain;
using HelpDesk.Application.Features.Tickets;
using MediatR;

namespace HelpDesk.Application.Features.Tickets.ChangeTicketStatus;

public sealed record ChangeTicketStatusCommand(TicketStatus Status)
    : IRequest<TicketResponse>
{
    public Guid Id { get; init; }
}
