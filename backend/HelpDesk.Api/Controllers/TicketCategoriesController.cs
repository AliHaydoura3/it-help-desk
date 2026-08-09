using HelpDesk.Application.Features.Admin;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/ticket-categories")]
public sealed class TicketCategoriesController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TicketCategorySettingResponse>>> GetActive(
        CancellationToken cancellationToken) =>
        Ok(await sender.Send(new GetTicketCategorySettingsQuery(ActiveOnly: true), cancellationToken));
}
