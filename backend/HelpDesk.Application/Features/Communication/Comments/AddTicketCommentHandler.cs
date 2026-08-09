using HelpDesk.Application.Abstractions.Communication;
using MediatR;

namespace HelpDesk.Application.Features.Communication.Comments;

public sealed class AddTicketCommentHandler(ICommunicationService communicationService)
    : IRequestHandler<AddTicketCommentCommand, TicketCommentResponse>
{
    public Task<TicketCommentResponse> Handle(AddTicketCommentCommand request, CancellationToken cancellationToken) =>
        communicationService.AddCommentAsync(request, cancellationToken);
}
