using HelpDesk.Domain;
using HelpDesk.Application.Features.Tickets;
using MediatR;

namespace HelpDesk.Application.Features.Tickets.Workflow;

public sealed record EscalateTicketCommand(
    TicketEscalationLevel Level,
    string Reason) : IRequest<TicketResponse>
{
    public Guid TicketId { get; init; }
}
