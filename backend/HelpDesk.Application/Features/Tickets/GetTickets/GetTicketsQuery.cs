using HelpDesk.Domain;
using MediatR;

namespace HelpDesk.Application.Features.Tickets.GetTickets;

public sealed record GetTicketsQuery(
    string? Search = null,
    TicketCategory? Category = null,
    TicketPriority? Priority = null,
    TicketStatus? Status = null,
    int PageNumber = 1,
    int PageSize = 10) : IRequest<GetTicketsResponse>;
