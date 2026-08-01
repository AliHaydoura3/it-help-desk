using HelpDesk.Domain;

namespace HelpDesk.Application.Features.Tickets.Workflow;

public sealed record AssignmentHistoryResponse(
    Guid? PreviousAgentId,
    Guid AssignedAgentId,
    Guid ActorUserId,
    TicketAssignmentType AssignmentType,
    DateTime OccurredAtUtc);

public sealed record InternalNoteResponse(
    Guid Id,
    Guid AuthorUserId,
    string Content,
    DateTime CreatedAtUtc);

public sealed record AssignableAgentResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    int ActiveTicketCount);
