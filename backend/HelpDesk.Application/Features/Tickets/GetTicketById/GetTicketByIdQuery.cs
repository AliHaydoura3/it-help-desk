using MediatR;
using HelpDesk.Application.Features.Tickets;

namespace HelpDesk.Application.Features.Tickets.GetTicketById;

public sealed record GetTicketByIdQuery(Guid Id) : IRequest<TicketResponse>;
