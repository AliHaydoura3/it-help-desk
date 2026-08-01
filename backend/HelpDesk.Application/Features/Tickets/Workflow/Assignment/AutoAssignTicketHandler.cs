using HelpDesk.Application.Abstractions.Tickets;
using HelpDesk.Application.Features.Tickets;
using MediatR;

namespace HelpDesk.Application.Features.Tickets.Workflow;

public sealed class AutoAssignTicketHandler(ITicketWorkflowService workflowService)
    : IRequestHandler<AutoAssignTicketCommand, TicketResponse>
{
    public Task<TicketResponse> Handle(AutoAssignTicketCommand request, CancellationToken cancellationToken) =>
        workflowService.AutoAssignAsync(request, cancellationToken);
}
