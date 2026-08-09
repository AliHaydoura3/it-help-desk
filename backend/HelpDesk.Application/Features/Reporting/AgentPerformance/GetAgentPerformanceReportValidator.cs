using FluentValidation;

namespace HelpDesk.Application.Features.Reporting.AgentPerformance;

public sealed class GetAgentPerformanceReportValidator : AbstractValidator<GetAgentPerformanceReportQuery>
{
    public GetAgentPerformanceReportValidator() =>
        RuleFor(query => query)
            .Must(query => !query.FromUtc.HasValue || !query.ToUtc.HasValue || query.FromUtc <= query.ToUtc)
            .WithMessage("FromUtc must be earlier than or equal to ToUtc.");
}
