using HelpDesk.Application.Abstractions.Communication;
using MediatR;

namespace HelpDesk.Application.Features.Communication.Mentions;

public sealed class GetMentionableAgentsHandler(ICommunicationService communicationService)
    : IRequestHandler<GetMentionableAgentsQuery, IReadOnlyList<MentionableAgentResponse>>
{
    public Task<IReadOnlyList<MentionableAgentResponse>> Handle(GetMentionableAgentsQuery request, CancellationToken cancellationToken) =>
        communicationService.GetMentionableAgentsAsync(request, cancellationToken);
}
