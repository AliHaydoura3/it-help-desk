using HelpDesk.Application.Abstractions.Attachments;
using MediatR;

namespace HelpDesk.Application.Features.Attachments.Policy;

public sealed class GetAttachmentPolicyHandler(IAttachmentPolicy policy)
    : IRequestHandler<GetAttachmentPolicyQuery, AttachmentPolicyResponse>
{
    public Task<AttachmentPolicyResponse> Handle(
        GetAttachmentPolicyQuery request,
        CancellationToken cancellationToken) =>
        Task.FromResult(new AttachmentPolicyResponse(
            policy.MaximumFileSizeBytes,
            policy.MaximumFilesPerTicket,
            policy.SupportedFileTypes
                .Select(fileType => new SupportedAttachmentTypeResponse(
                    fileType.Extension,
                    fileType.ContentType))
                .ToList()));
}
