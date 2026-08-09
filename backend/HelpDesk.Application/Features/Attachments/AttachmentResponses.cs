namespace HelpDesk.Application.Features.Attachments;

public sealed record AttachmentUploaderResponse(
    Guid Id,
    string FirstName,
    string LastName);

public sealed record TicketAttachmentResponse(
    Guid Id,
    Guid TicketId,
    string FileName,
    string ContentType,
    string Extension,
    long SizeBytes,
    string Sha256Hash,
    AttachmentUploaderResponse UploadedBy,
    DateTime UploadedAtUtc);
