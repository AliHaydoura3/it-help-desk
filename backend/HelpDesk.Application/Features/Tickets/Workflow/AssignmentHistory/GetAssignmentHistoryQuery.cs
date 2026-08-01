using MediatR;

namespace HelpDesk.Application.Features.Tickets.Workflow;

public sealed record GetAssignmentHistoryQuery(Guid TicketId)
    : IRequest<IReadOnlyList<AssignmentHistoryResponse>>;
