using HelpDesk.Application.Features.Communication;
using MediatR;

namespace HelpDesk.Application.Features.Communication.Mentions;

public sealed record GetMentionableAgentsQuery(
    Guid TicketId,
    string? Search = null,
    int Limit = 20) : IRequest<IReadOnlyList<MentionableAgentResponse>>;
