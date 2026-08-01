using MediatR;

namespace HelpDesk.Application.Features.Tickets.Workflow;

public sealed record GetInternalNotesQuery(Guid TicketId)
    : IRequest<IReadOnlyList<InternalNoteResponse>>;
