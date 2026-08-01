using MediatR;

namespace HelpDesk.Application.Features.Tickets.Workflow;

public sealed record AddInternalNoteCommand(string Content)
    : IRequest<InternalNoteResponse>
{
    public Guid TicketId { get; init; }
}
