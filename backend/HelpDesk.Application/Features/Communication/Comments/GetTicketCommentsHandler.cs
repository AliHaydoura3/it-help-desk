using HelpDesk.Application.Abstractions.Communication;
using MediatR;

namespace HelpDesk.Application.Features.Communication.Comments;

public sealed class GetTicketCommentsHandler(ICommunicationService communicationService)
    : IRequestHandler<GetTicketCommentsQuery, GetTicketCommentsResponse>
{
    public Task<GetTicketCommentsResponse> Handle(GetTicketCommentsQuery request, CancellationToken cancellationToken) =>
        communicationService.GetCommentsAsync(request, cancellationToken);
}
