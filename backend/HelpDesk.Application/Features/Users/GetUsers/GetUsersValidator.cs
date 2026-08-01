using FluentValidation;

namespace HelpDesk.Application.Features.Users.GetUsers;

public sealed class GetUsersValidator : AbstractValidator<GetUsersQuery>
{
    public GetUsersValidator()
    {
        RuleFor(query => query.PageNumber)
            .GreaterThanOrEqualTo(1);

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(query => query.CurrentUserId)
            .NotEmpty();

        RuleFor(query => query.Search)
            .MaximumLength(200);
    }
}
