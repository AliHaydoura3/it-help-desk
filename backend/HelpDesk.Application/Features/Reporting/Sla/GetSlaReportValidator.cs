using FluentValidation;

namespace HelpDesk.Application.Features.Reporting.Sla;

public sealed class GetSlaReportValidator : AbstractValidator<GetSlaReportQuery>
{
    public GetSlaReportValidator() =>
        RuleFor(query => query)
            .Must(query => !query.FromUtc.HasValue || !query.ToUtc.HasValue || query.FromUtc <= query.ToUtc)
            .WithMessage("FromUtc must be earlier than or equal to ToUtc.");
}
