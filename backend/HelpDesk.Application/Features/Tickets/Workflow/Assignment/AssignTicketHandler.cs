using HelpDesk.Application.Abstractions.Tickets;
using HelpDesk.Application.Features.Tickets;
using MediatR;

namespace HelpDesk.Application.Features.Tickets.Workflow;

public sealed class AssignTicketHandler(ITicketWorkflowService workflowService)
    : IRequestHandler<AssignTicketCommand, TicketResponse>
{
    public Task<TicketResponse> Handle(AssignTicketCommand request, CancellationToken cancellationToken) =>
        workflowService.AssignAsync(request, cancellationToken);
}
