using HelpDesk.Application.Features.Attachments;
using HelpDesk.Application.Features.Attachments.Download;
using HelpDesk.Application.Features.Attachments.List;
using HelpDesk.Application.Features.Attachments.Upload;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/tickets/{ticketId:guid}/attachments")]
public sealed class TicketAttachmentsController(ISender sender) : ControllerBase
{
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<TicketAttachmentResponse>> Upload(
        Guid ticketId,
        [FromForm] UploadTicketAttachmentRequest request,
        CancellationToken cancellationToken)
    {
        await using var content = request.File.OpenReadStream();
        var response = await sender.Send(
            new UploadTicketAttachmentCommand(
                ticketId,
                request.File.FileName,
                request.File.ContentType,
                request.File.Length,
                content),
            cancellationToken);

        return CreatedAtAction(
            nameof(Download),
            new { ticketId, attachmentId = response.Id },
            response);
    }

    [HttpGet]
    public async Task<ActionResult<GetTicketAttachmentsResponse>> GetAll(
        Guid ticketId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        Ok(await sender.Send(
            new GetTicketAttachmentsQuery(ticketId, pageNumber, pageSize),
            cancellationToken));

    [HttpGet("{attachmentId:guid}/download")]
    public async Task<IActionResult> Download(
        Guid ticketId,
        Guid attachmentId,
        CancellationToken cancellationToken)
    {
        var attachment = await sender.Send(
            new DownloadTicketAttachmentQuery(ticketId, attachmentId),
            cancellationToken);
        Response.Headers["Cache-Control"] = "private, no-store";
        Response.Headers["Content-Security-Policy"] = "default-src 'none'; sandbox";
        Response.Headers["ETag"] = $"\"{attachment.Sha256Hash}\"";
        Response.Headers["Last-Modified"] = attachment.UploadedAtUtc.ToString("R");
        Response.Headers["X-Content-Type-Options"] = "nosniff";

        return File(
            attachment.Content,
            attachment.ContentType,
            attachment.FileName,
            enableRangeProcessing: true);
    }
}

public sealed record UploadTicketAttachmentRequest(IFormFile File);
