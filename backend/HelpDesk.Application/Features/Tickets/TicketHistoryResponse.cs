namespace HelpDesk.Application.Features.Tickets;

public sealed record TicketHistoryResponse(
    string Action,
    string? PreviousValue,
    string? NewValue,
    Guid ActorUserId,
    DateTime OccurredAtUtc);
