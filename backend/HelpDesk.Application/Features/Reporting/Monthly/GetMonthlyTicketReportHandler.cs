using HelpDesk.Application.Abstractions.Reporting;
using MediatR;

namespace HelpDesk.Application.Features.Reporting.Monthly;

public sealed class GetMonthlyTicketReportHandler(IReportingService reportingService)
    : IRequestHandler<GetMonthlyTicketReportQuery, MonthlyTicketReportResponse>
{
    public Task<MonthlyTicketReportResponse> Handle(
        GetMonthlyTicketReportQuery request,
        CancellationToken cancellationToken) =>
        reportingService.GetMonthlyReportAsync(request, cancellationToken);
}
