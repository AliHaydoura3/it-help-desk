using HelpDesk.Application.Common.Authorization;
using HelpDesk.Application.Features.Admin;
using HelpDesk.Domain;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Api.Controllers;

[ApiController]
[Authorize(Policy = Policies.AdminOnly)]
[Route("api/admin")]
public sealed class AdminController(ISender sender) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<ActionResult<AdminDashboardResponse>> GetDashboard(CancellationToken cancellationToken) =>
        Ok(await sender.Send(new GetAdminDashboardQuery(), cancellationToken));

    [HttpGet("roles")]
    public async Task<ActionResult<IReadOnlyList<AdminRoleResponse>>> GetRoles(CancellationToken cancellationToken) =>
        Ok(await sender.Send(new GetAdminRolesQuery(), cancellationToken));

    [HttpGet("ticket-categories")]
    public async Task<ActionResult<IReadOnlyList<TicketCategorySettingResponse>>> GetCategories(CancellationToken cancellationToken) =>
        Ok(await sender.Send(new GetTicketCategorySettingsQuery(), cancellationToken));

    [HttpPut("ticket-categories/{category}")]
    public async Task<ActionResult<TicketCategorySettingResponse>> UpdateCategory(
        TicketCategory category,
        [FromBody] UpdateTicketCategoryCommand command,
        CancellationToken cancellationToken) =>
        Ok(await sender.Send(command with { Category = category }, cancellationToken));

    [HttpGet("settings")]
    public async Task<ActionResult<SystemSettingsResponse>> GetSettings(CancellationToken cancellationToken) =>
        Ok(await sender.Send(new GetSystemSettingsQuery(), cancellationToken));

    [HttpPut("settings")]
    public async Task<ActionResult<SystemSettingsResponse>> UpdateSettings(
        [FromBody] UpdateSystemSettingsCommand command,
        CancellationToken cancellationToken) =>
        Ok(await sender.Send(command, cancellationToken));
}
