using HelpDesk.Application.Features.Communication;
using HelpDesk.Application.Features.Communication.Notifications;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class NotificationsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<GetNotificationsResponse>> GetAll(
        [FromQuery] GetNotificationsQuery query,
        CancellationToken cancellationToken) =>
        Ok(await sender.Send(query, cancellationToken));

    [HttpGet("unread-count")]
    public async Task<ActionResult<UnreadNotificationCountResponse>> GetUnreadCount(
        CancellationToken cancellationToken) =>
        Ok(await sender.Send(new GetUnreadNotificationCountQuery(), cancellationToken));

    [HttpPatch("{id:guid}/read")]
    public async Task<ActionResult<NotificationResponse>> MarkRead(
        Guid id,
        CancellationToken cancellationToken) =>
        Ok(await sender.Send(new MarkNotificationReadCommand(id), cancellationToken));

    [HttpPost("read-all")]
    public async Task<ActionResult<MarkAllNotificationsReadResponse>> MarkAllRead(
        CancellationToken cancellationToken) =>
        Ok(await sender.Send(new MarkAllNotificationsReadCommand(), cancellationToken));
}
