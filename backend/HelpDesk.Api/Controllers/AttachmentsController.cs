using HelpDesk.Application.Features.Attachments.Policy;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/attachments")]
public sealed class AttachmentsController(ISender sender) : ControllerBase
{
    [HttpGet("policy")]
    public async Task<ActionResult<AttachmentPolicyResponse>> GetPolicy(
        CancellationToken cancellationToken) =>
        Ok(await sender.Send(new GetAttachmentPolicyQuery(), cancellationToken));
}
