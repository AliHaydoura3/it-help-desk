using HelpDesk.Domain;

namespace HelpDesk.Application.Features.Tickets;

public sealed record TicketResponse(
    Guid Id,
    string ReferenceNumber,
    string Title,
    string Description,
    TicketCategory Category,
    TicketPriority Priority,
    TicketStatus Status,
    TicketEscalationLevel EscalationLevel,
    bool IsCancelled,
    Guid CreatedByUserId,
    Guid? AssignedToUserId,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
