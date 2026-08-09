using FluentValidation;

namespace HelpDesk.Application.Features.Reporting.Dashboard;

public sealed class GetDashboardReportValidator : AbstractValidator<GetDashboardReportQuery>
{
    public GetDashboardReportValidator() =>
        RuleFor(query => query)
            .Must(query => !query.FromUtc.HasValue || !query.ToUtc.HasValue || query.FromUtc <= query.ToUtc)
            .WithMessage("FromUtc must be earlier than or equal to ToUtc.");
}
