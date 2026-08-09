using FluentValidation;

namespace HelpDesk.Application.Features.Reporting.Monthly;

public sealed class GetMonthlyTicketReportValidator : AbstractValidator<GetMonthlyTicketReportQuery>
{
    public GetMonthlyTicketReportValidator() =>
        RuleFor(query => query.Months).InclusiveBetween(1, 60);
}
