using MediatR;

namespace HelpDesk.Application.Features.Attachments.List;

public sealed record GetTicketAttachmentsQuery(
    Guid TicketId,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<GetTicketAttachmentsResponse>;

public sealed record GetTicketAttachmentsResponse(
    IReadOnlyList<TicketAttachmentResponse> Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages);
