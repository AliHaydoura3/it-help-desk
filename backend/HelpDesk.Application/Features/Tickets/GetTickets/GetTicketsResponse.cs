using HelpDesk.Application.Features.Tickets;

namespace HelpDesk.Application.Features.Tickets.GetTickets;

public sealed record GetTicketsResponse(
    IReadOnlyList<TicketResponse> Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages);
