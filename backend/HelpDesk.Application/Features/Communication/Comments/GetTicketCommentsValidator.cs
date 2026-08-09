using FluentValidation;

namespace HelpDesk.Application.Features.Communication.Comments;

public sealed class GetTicketCommentsValidator : AbstractValidator<GetTicketCommentsQuery>
{
    public GetTicketCommentsValidator()
    {
        RuleFor(query => query.TicketId).NotEmpty();
        RuleFor(query => query.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
    }
}
