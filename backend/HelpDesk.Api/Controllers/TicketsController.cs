using HelpDesk.Application.Features.Tickets;
using HelpDesk.Application.Features.Tickets.CancelTicket;
using HelpDesk.Application.Features.Tickets.ChangeTicketStatus;
using HelpDesk.Application.Features.Tickets.CreateTicket;
using HelpDesk.Application.Features.Tickets.GetTicketById;
using HelpDesk.Application.Features.Tickets.GetTicketHistory;
using HelpDesk.Application.Features.Tickets.GetTicketSummary;
using HelpDesk.Application.Features.Tickets.GetTickets;
using HelpDesk.Application.Features.Tickets.UpdateTicket;
using HelpDesk.Application.Features.Tickets.Workflow;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class TicketsController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<TicketResponse>> Create(
        [FromBody] CreateTicketCommand command,
        CancellationToken cancellationToken)
    {
        var response = await sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpGet]
    public async Task<ActionResult<GetTicketsResponse>> GetAll(
        [FromQuery] GetTicketsQuery query,
        CancellationToken cancellationToken) =>
        Ok(await sender.Send(query, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TicketResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken) =>
        Ok(await sender.Send(new GetTicketByIdQuery(id), cancellationToken));

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TicketResponse>> Update(
        Guid id,
        [FromBody] UpdateTicketCommand command,
        CancellationToken cancellationToken) =>
        Ok(await sender.Send(command with { Id = id }, cancellationToken));

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<TicketResponse>> ChangeStatus(
        Guid id,
        [FromBody] ChangeTicketStatusCommand command,
        CancellationToken cancellationToken) =>
        Ok(await sender.Send(command with { Id = id }, cancellationToken));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new CancelTicketCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:guid}/history")]
    public async Task<ActionResult<IReadOnlyList<TicketHistoryResponse>>> GetHistory(
        Guid id,
        CancellationToken cancellationToken) =>
        Ok(await sender.Send(new GetTicketHistoryQuery(id), cancellationToken));

    [HttpGet("reports/summary")]
    public async Task<ActionResult<TicketSummaryResponse>> GetSummary(
        CancellationToken cancellationToken) =>
        Ok(await sender.Send(new GetTicketSummaryQuery(), cancellationToken));

    [HttpGet("assignable-agents")]
    public async Task<ActionResult<IReadOnlyList<AssignableAgentResponse>>> GetAssignableAgents(
        CancellationToken cancellationToken) =>
        Ok(await sender.Send(new GetAssignableAgentsQuery(), cancellationToken));

    [HttpPost("{id:guid}/assign")]
    public async Task<ActionResult<TicketResponse>> Assign(
        Guid id,
        [FromBody] AssignTicketCommand command,
        CancellationToken cancellationToken) =>
        Ok(await sender.Send(command with { TicketId = id }, cancellationToken));

    [HttpPost("{id:guid}/auto-assign")]
    public async Task<ActionResult<TicketResponse>> AutoAssign(
        Guid id,
        CancellationToken cancellationToken) =>
        Ok(await sender.Send(new AutoAssignTicketCommand(id), cancellationToken));

    [HttpPost("{id:guid}/escalate")]
    public async Task<ActionResult<TicketResponse>> Escalate(
        Guid id,
        [FromBody] EscalateTicketCommand command,
        CancellationToken cancellationToken) =>
        Ok(await sender.Send(command with { TicketId = id }, cancellationToken));

    [HttpGet("{id:guid}/assignments")]
    public async Task<ActionResult<IReadOnlyList<AssignmentHistoryResponse>>> GetAssignments(
        Guid id,
        CancellationToken cancellationToken) =>
        Ok(await sender.Send(new GetAssignmentHistoryQuery(id), cancellationToken));

    [HttpGet("{id:guid}/internal-notes")]
    public async Task<ActionResult<IReadOnlyList<InternalNoteResponse>>> GetInternalNotes(
        Guid id,
        CancellationToken cancellationToken) =>
        Ok(await sender.Send(new GetInternalNotesQuery(id), cancellationToken));

    [HttpPost("{id:guid}/internal-notes")]
    public async Task<ActionResult<InternalNoteResponse>> AddInternalNote(
        Guid id,
        [FromBody] AddInternalNoteCommand command,
        CancellationToken cancellationToken)
    {
        var response = await sender.Send(command with { TicketId = id }, cancellationToken);
        return CreatedAtAction(nameof(GetInternalNotes), new { id }, response);
    }
}
