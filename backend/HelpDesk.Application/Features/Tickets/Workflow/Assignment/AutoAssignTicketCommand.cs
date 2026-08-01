using MediatR;
using HelpDesk.Application.Features.Tickets;

namespace HelpDesk.Application.Features.Tickets.Workflow;

public sealed record AutoAssignTicketCommand(Guid TicketId) : IRequest<TicketResponse>;
