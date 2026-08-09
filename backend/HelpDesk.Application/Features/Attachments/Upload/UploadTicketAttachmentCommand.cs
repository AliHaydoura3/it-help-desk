using HelpDesk.Application.Features.Attachments;
using MediatR;

namespace HelpDesk.Application.Features.Attachments.Upload;

public sealed record UploadTicketAttachmentCommand(
    Guid TicketId,
    string FileName,
    string ContentType,
    long DeclaredSizeBytes,
    Stream Content) : IRequest<TicketAttachmentResponse>;
