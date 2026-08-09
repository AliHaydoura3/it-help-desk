using HelpDesk.Application.Features.Communication;
using MediatR;

namespace HelpDesk.Application.Features.Communication.Comments;

public sealed record AddTicketCommentCommand(
    Guid TicketId,
    string Content,
    Guid? ParentCommentId = null,
    IReadOnlyCollection<Guid>? MentionedAgentIds = null) : IRequest<TicketCommentResponse>;
