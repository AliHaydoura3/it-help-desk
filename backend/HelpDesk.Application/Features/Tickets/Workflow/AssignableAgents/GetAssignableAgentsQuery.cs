using MediatR;

namespace HelpDesk.Application.Features.Tickets.Workflow;

public sealed record GetAssignableAgentsQuery : IRequest<IReadOnlyList<AssignableAgentResponse>>;
