using MediatR;

namespace HelpDesk.Application.Features.Attachments.Policy;

public sealed record GetAttachmentPolicyQuery : IRequest<AttachmentPolicyResponse>;

public sealed record AttachmentPolicyResponse(
    long MaximumFileSizeBytes,
    int MaximumFilesPerTicket,
    IReadOnlyList<SupportedAttachmentTypeResponse> SupportedTypes);

public sealed record SupportedAttachmentTypeResponse(
    string Extension,
    string ContentType);
