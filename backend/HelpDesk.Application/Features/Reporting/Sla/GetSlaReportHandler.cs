using HelpDesk.Application.Abstractions.Reporting;
using MediatR;

namespace HelpDesk.Application.Features.Reporting.Sla;

public sealed class GetSlaReportHandler(IReportingService reportingService)
    : IRequestHandler<GetSlaReportQuery, SlaReportResponse>
{
    public Task<SlaReportResponse> Handle(
        GetSlaReportQuery request,
        CancellationToken cancellationToken) =>
        reportingService.GetSlaReportAsync(request, cancellationToken);
}
