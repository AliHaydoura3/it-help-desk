using MediatR;

namespace HelpDesk.Application.Features.Tickets.GetTicketSummary;

public sealed record GetTicketSummaryQuery : IRequest<TicketSummaryResponse>;
