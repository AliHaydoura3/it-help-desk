using HelpDesk.Application.Features.Communication;
using HelpDesk.Application.Features.Communication.Comments;
using HelpDesk.Application.Features.Communication.Mentions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/tickets/{ticketId:guid}")]
public sealed class TicketCommunicationController(ISender sender) : ControllerBase
{
    [HttpGet("comments")]
    public async Task<ActionResult<GetTicketCommentsResponse>> GetComments(
        Guid ticketId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        Ok(await sender.Send(
            new GetTicketCommentsQuery(ticketId, pageNumber, pageSize),
            cancellationToken));

    [HttpPost("comments")]
    public async Task<ActionResult<TicketCommentResponse>> AddComment(
        Guid ticketId,
        [FromBody] AddTicketCommentRequest request,
        CancellationToken cancellationToken)
    {
        var response = await sender.Send(
            new AddTicketCommentCommand(
                ticketId,
                request.Content,
                request.ParentCommentId,
                request.MentionedAgentIds),
            cancellationToken);
        return CreatedAtAction(
            nameof(GetComments),
            new { ticketId },
            response);
    }

    [HttpGet("mentionable-agents")]
    public async Task<ActionResult<IReadOnlyList<MentionableAgentResponse>>> GetMentionableAgents(
        Guid ticketId,
        [FromQuery] string? search = null,
        [FromQuery] int limit = 20,
        CancellationToken cancellationToken = default) =>
        Ok(await sender.Send(
            new GetMentionableAgentsQuery(ticketId, search, limit),
            cancellationToken));
}

public sealed record AddTicketCommentRequest(
    string Content,
    Guid? ParentCommentId,
    IReadOnlyCollection<Guid>? MentionedAgentIds);
