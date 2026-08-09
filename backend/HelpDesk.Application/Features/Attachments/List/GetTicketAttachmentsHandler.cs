using HelpDesk.Application.Abstractions.Attachments;
using MediatR;

namespace HelpDesk.Application.Features.Attachments.List;

public sealed class GetTicketAttachmentsHandler(
    ITicketAttachmentService attachmentService)
    : IRequestHandler<GetTicketAttachmentsQuery, GetTicketAttachmentsResponse>
{
    public Task<GetTicketAttachmentsResponse> Handle(
        GetTicketAttachmentsQuery request,
        CancellationToken cancellationToken) =>
        attachmentService.GetAllAsync(request, cancellationToken);
}
