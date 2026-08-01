using HelpDesk.Application.Abstractions.Tickets;
using MediatR;

namespace HelpDesk.Application.Features.Tickets.Workflow;

public sealed class GetAssignmentHistoryHandler(ITicketWorkflowService workflowService)
    : IRequestHandler<GetAssignmentHistoryQuery, IReadOnlyList<AssignmentHistoryResponse>>
{
    public Task<IReadOnlyList<AssignmentHistoryResponse>> Handle(GetAssignmentHistoryQuery request, CancellationToken cancellationToken) =>
        workflowService.GetAssignmentHistoryAsync(request, cancellationToken);
}
