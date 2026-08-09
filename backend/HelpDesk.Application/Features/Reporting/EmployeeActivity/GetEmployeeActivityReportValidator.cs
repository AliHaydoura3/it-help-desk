using FluentValidation;
using HelpDesk.Application.Common.Authorization;

namespace HelpDesk.Application.Features.Reporting.EmployeeActivity;

public sealed class GetEmployeeActivityReportValidator : AbstractValidator<GetEmployeeActivityReportQuery>
{
    public GetEmployeeActivityReportValidator()
    {
        RuleFor(query => query)
            .Must(query => !query.FromUtc.HasValue || !query.ToUtc.HasValue || query.FromUtc <= query.ToUtc)
            .WithMessage("FromUtc must be earlier than or equal to ToUtc.");
        RuleFor(query => query.PageNumber).GreaterThan(0);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
        RuleFor(query => query.Role)
            .Must(role => string.IsNullOrWhiteSpace(role) || Roles.IsDefined(role))
            .WithMessage("Role must be one of the four supported system roles.");
    }
}
