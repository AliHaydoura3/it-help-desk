using HelpDesk.Application.Abstractions.Attachments;
using MediatR;

namespace HelpDesk.Application.Features.Attachments.Download;

public sealed class DownloadTicketAttachmentHandler(
    ITicketAttachmentService attachmentService)
    : IRequestHandler<DownloadTicketAttachmentQuery, AttachmentDownloadResponse>
{
    public Task<AttachmentDownloadResponse> Handle(
        DownloadTicketAttachmentQuery request,
        CancellationToken cancellationToken) =>
        attachmentService.DownloadAsync(request, cancellationToken);
}
