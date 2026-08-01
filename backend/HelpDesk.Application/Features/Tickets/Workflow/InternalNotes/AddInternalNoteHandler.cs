using HelpDesk.Application.Abstractions.Tickets;
using MediatR;

namespace HelpDesk.Application.Features.Tickets.Workflow;

public sealed class AddInternalNoteHandler(ITicketWorkflowService workflowService)
    : IRequestHandler<AddInternalNoteCommand, InternalNoteResponse>
{
    public Task<InternalNoteResponse> Handle(AddInternalNoteCommand request, CancellationToken cancellationToken) =>
        workflowService.AddInternalNoteAsync(request, cancellationToken);
}
