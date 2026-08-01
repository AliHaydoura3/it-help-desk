using HelpDesk.Application.Abstractions.Tickets;
using HelpDesk.Application.Features.Tickets;
using MediatR;

namespace HelpDesk.Application.Features.Tickets.Workflow;

public sealed class EscalateTicketHandler(ITicketWorkflowService workflowService)
    : IRequestHandler<EscalateTicketCommand, TicketResponse>
{
    public Task<TicketResponse> Handle(EscalateTicketCommand request, CancellationToken cancellationToken) =>
        workflowService.EscalateAsync(request, cancellationToken);
}
