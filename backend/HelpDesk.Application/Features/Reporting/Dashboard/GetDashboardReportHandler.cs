using HelpDesk.Application.Abstractions.Reporting;
using MediatR;

namespace HelpDesk.Application.Features.Reporting.Dashboard;

public sealed class GetDashboardReportHandler(IReportingService reportingService)
    : IRequestHandler<GetDashboardReportQuery, DashboardReportResponse>
{
    public Task<DashboardReportResponse> Handle(
        GetDashboardReportQuery request,
        CancellationToken cancellationToken) =>
        reportingService.GetDashboardAsync(request, cancellationToken);
}
