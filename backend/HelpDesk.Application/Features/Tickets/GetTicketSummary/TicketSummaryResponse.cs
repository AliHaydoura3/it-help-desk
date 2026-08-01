namespace HelpDesk.Application.Features.Tickets.GetTicketSummary;

public sealed record TicketSummaryResponse(
    int Total,
    int Open,
    int InProgress,
    int Pending,
    int Resolved,
    int Closed,
    int Critical);
