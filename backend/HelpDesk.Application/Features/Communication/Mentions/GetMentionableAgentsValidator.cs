using FluentValidation;

namespace HelpDesk.Application.Features.Communication.Mentions;

public sealed class GetMentionableAgentsValidator : AbstractValidator<GetMentionableAgentsQuery>
{
    public GetMentionableAgentsValidator()
    {
        RuleFor(query => query.TicketId).NotEmpty();
        RuleFor(query => query.Search).MaximumLength(100);
        RuleFor(query => query.Limit).InclusiveBetween(1, 50);
    }
}
