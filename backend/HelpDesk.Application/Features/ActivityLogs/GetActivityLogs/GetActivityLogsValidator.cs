using FluentValidation;

namespace HelpDesk.Application.Features.ActivityLogs.GetActivityLogs;

public sealed class GetActivityLogsValidator
    : AbstractValidator<GetActivityLogsQuery>
{
    public GetActivityLogsValidator()
    {
        RuleFor(query => query.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
    }
}
