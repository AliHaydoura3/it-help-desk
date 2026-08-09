using HelpDesk.Application.Abstractions.Attachments;
using HelpDesk.Application.Features.Attachments;
using MediatR;

namespace HelpDesk.Application.Features.Attachments.Upload;

public sealed class UploadTicketAttachmentHandler(
    ITicketAttachmentService attachmentService)
    : IRequestHandler<UploadTicketAttachmentCommand, TicketAttachmentResponse>
{
    public Task<TicketAttachmentResponse> Handle(
        UploadTicketAttachmentCommand request,
        CancellationToken cancellationToken) =>
        attachmentService.UploadAsync(request, cancellationToken);
}
