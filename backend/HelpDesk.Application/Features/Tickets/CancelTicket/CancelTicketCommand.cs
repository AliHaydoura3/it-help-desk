using MediatR;

namespace HelpDesk.Application.Features.Tickets.CancelTicket;

public sealed record CancelTicketCommand(Guid Id) : IRequest;
