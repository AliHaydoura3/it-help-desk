using FluentValidation;

namespace HelpDesk.Application.Features.Tickets.GetTickets;

public sealed class GetTicketsValidator : AbstractValidator<GetTicketsQuery>
{
    public GetTicketsValidator()
    {
        RuleFor(query => query.Search).MaximumLength(200);
        RuleFor(query => query.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
        RuleFor(query => query.Category).IsInEnum().When(query => query.Category.HasValue);
        RuleFor(query => query.Priority).IsInEnum().When(query => query.Priority.HasValue);
        RuleFor(query => query.Status).IsInEnum().When(query => query.Status.HasValue);
    }
}
