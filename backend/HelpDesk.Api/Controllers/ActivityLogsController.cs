using HelpDesk.Application.Common.Authorization;
using HelpDesk.Application.Features.ActivityLogs.GetActivityLogs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Api.Controllers;

[ApiController]
[Authorize(Policy = Policies.AdminOnly)]
[Route("api/activity-logs")]
public sealed class ActivityLogsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<GetActivityLogsResponse>> Get(
        [FromQuery] GetActivityLogsQuery query,
        CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(query, cancellationToken));
    }
}
