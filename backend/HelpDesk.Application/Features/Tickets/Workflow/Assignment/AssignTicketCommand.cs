using MediatR;
using HelpDesk.Application.Features.Tickets;

namespace HelpDesk.Application.Features.Tickets.Workflow;

public sealed record AssignTicketCommand(Guid AgentUserId) : IRequest<TicketResponse>
{
    public Guid TicketId { get; init; }
}
