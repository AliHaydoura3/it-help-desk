using FluentValidation;
using HelpDesk.Application.Common.Authorization;

namespace HelpDesk.Application.Features.Reporting.Exports;

public sealed class ExportReportValidator : AbstractValidator<ExportReportQuery>
{
    public ExportReportValidator()
    {
        RuleFor(query => query.Type).IsInEnum();
        RuleFor(query => query.Format).IsInEnum();
        RuleFor(query => query.Months).InclusiveBetween(1, 60);
        RuleFor(query => query)
            .Must(query => !query.FromUtc.HasValue || !query.ToUtc.HasValue || query.FromUtc <= query.ToUtc)
            .WithMessage("FromUtc must be earlier than or equal to ToUtc.");
        RuleFor(query => query.Role)
            .Must(role => string.IsNullOrWhiteSpace(role) || Roles.IsDefined(role))
            .WithMessage("Role must be one of the four supported system roles.");
    }
}
