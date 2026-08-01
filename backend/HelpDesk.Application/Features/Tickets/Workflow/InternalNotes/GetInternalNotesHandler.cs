using HelpDesk.Application.Abstractions.Tickets;
using MediatR;

namespace HelpDesk.Application.Features.Tickets.Workflow;

public sealed class GetInternalNotesHandler(ITicketWorkflowService workflowService)
    : IRequestHandler<GetInternalNotesQuery, IReadOnlyList<InternalNoteResponse>>
{
    public Task<IReadOnlyList<InternalNoteResponse>> Handle(GetInternalNotesQuery request, CancellationToken cancellationToken) =>
        workflowService.GetInternalNotesAsync(request, cancellationToken);
}
