using HelpDesk.Application.Features.Tickets;
using HelpDesk.Application.Features.Tickets.CancelTicket;
using HelpDesk.Application.Features.Tickets.ChangeTicketStatus;
using HelpDesk.Application.Features.Tickets.CreateTicket;
using HelpDesk.Application.Features.Tickets.GetTicketById;
using HelpDesk.Application.Features.Tickets.GetTicketHistory;
using HelpDesk.Application.Features.Tickets.GetTicketSummary;
using HelpDesk.Application.Features.Tickets.GetTickets;
using HelpDesk.Application.Features.Tickets.UpdateTicket;

namespace HelpDesk.Application.Abstractions.Tickets;

public interface ITicketService
{
    Task<TicketResponse> CreateAsync(CreateTicketCommand command, CancellationToken cancellationToken);
    Task<GetTicketsResponse> GetAllAsync(GetTicketsQuery query, CancellationToken cancellationToken);
    Task<TicketResponse> GetByIdAsync(GetTicketByIdQuery query, CancellationToken cancellationToken);
    Task<TicketResponse> UpdateAsync(UpdateTicketCommand command, CancellationToken cancellationToken);
    Task<TicketResponse> ChangeStatusAsync(ChangeTicketStatusCommand command, CancellationToken cancellationToken);
    Task CancelAsync(CancelTicketCommand command, CancellationToken cancellationToken);
    Task<IReadOnlyList<TicketHistoryResponse>> GetHistoryAsync(GetTicketHistoryQuery query, CancellationToken cancellationToken);
    Task<TicketSummaryResponse> GetSummaryAsync(GetTicketSummaryQuery query, CancellationToken cancellationToken);
}
