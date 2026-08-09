using HelpDesk.Application.Abstractions.Reporting;
using MediatR;

namespace HelpDesk.Application.Features.Reporting.AgentPerformance;

public sealed class GetAgentPerformanceReportHandler(IReportingService reportingService)
    : IRequestHandler<GetAgentPerformanceReportQuery, AgentPerformanceReportResponse>
{
    public Task<AgentPerformanceReportResponse> Handle(
        GetAgentPerformanceReportQuery request,
        CancellationToken cancellationToken) =>
        reportingService.GetAgentPerformanceAsync(request, cancellationToken);
}
