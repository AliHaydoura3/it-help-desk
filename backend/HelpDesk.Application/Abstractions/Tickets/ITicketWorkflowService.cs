using HelpDesk.Application.Features.Tickets;
using HelpDesk.Application.Features.Tickets.Workflow;

namespace HelpDesk.Application.Abstractions.Tickets;

public interface ITicketWorkflowService
{
    Task<TicketResponse> AssignAsync(AssignTicketCommand command, CancellationToken cancellationToken);
    Task<TicketResponse> AutoAssignAsync(AutoAssignTicketCommand command, CancellationToken cancellationToken);
    Task<TicketResponse> EscalateAsync(EscalateTicketCommand command, CancellationToken cancellationToken);
    Task<InternalNoteResponse> AddInternalNoteAsync(AddInternalNoteCommand command, CancellationToken cancellationToken);
    Task<IReadOnlyList<InternalNoteResponse>> GetInternalNotesAsync(GetInternalNotesQuery query, CancellationToken cancellationToken);
    Task<IReadOnlyList<AssignmentHistoryResponse>> GetAssignmentHistoryAsync(GetAssignmentHistoryQuery query, CancellationToken cancellationToken);
    Task<IReadOnlyList<AssignableAgentResponse>> GetAssignableAgentsAsync(GetAssignableAgentsQuery query, CancellationToken cancellationToken);
}
