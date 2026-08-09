using MediatR;

namespace HelpDesk.Application.Features.Attachments.Download;

public sealed record DownloadTicketAttachmentQuery(
    Guid TicketId,
    Guid AttachmentId) : IRequest<AttachmentDownloadResponse>;

public sealed record AttachmentDownloadResponse(
    Stream Content,
    string FileName,
    string ContentType,
    long SizeBytes,
    string Sha256Hash,
    DateTime UploadedAtUtc);
