using HelpDesk.Application.Abstractions.Tickets;
using MediatR;

namespace HelpDesk.Application.Features.Tickets.Workflow;

public sealed class GetAssignableAgentsHandler(ITicketWorkflowService workflowService)
    : IRequestHandler<GetAssignableAgentsQuery, IReadOnlyList<AssignableAgentResponse>>
{
    public Task<IReadOnlyList<AssignableAgentResponse>> Handle(GetAssignableAgentsQuery request, CancellationToken cancellationToken) =>
        workflowService.GetAssignableAgentsAsync(request, cancellationToken);
}
