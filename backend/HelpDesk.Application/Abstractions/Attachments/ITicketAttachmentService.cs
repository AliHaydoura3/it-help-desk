using HelpDesk.Application.Features.Attachments;
using HelpDesk.Application.Features.Attachments.Download;
using HelpDesk.Application.Features.Attachments.List;
using HelpDesk.Application.Features.Attachments.Upload;

namespace HelpDesk.Application.Abstractions.Attachments;

public interface ITicketAttachmentService
{
    Task<TicketAttachmentResponse> UploadAsync(
        UploadTicketAttachmentCommand command,
        CancellationToken cancellationToken);

    Task<GetTicketAttachmentsResponse> GetAllAsync(
        GetTicketAttachmentsQuery query,
        CancellationToken cancellationToken);

    Task<AttachmentDownloadResponse> DownloadAsync(
        DownloadTicketAttachmentQuery query,
        CancellationToken cancellationToken);
}
