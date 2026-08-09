using MediatR;

namespace HelpDesk.Application.Features.Reporting.AgentPerformance;

public sealed record GetAgentPerformanceReportQuery(
    DateTime? FromUtc = null,
    DateTime? ToUtc = null) : IRequest<AgentPerformanceReportResponse>;

public sealed record AgentPerformanceReportResponse(
    DateTime GeneratedAtUtc,
    ReportingPeriodResponse Period,
    IReadOnlyList<AgentPerformanceItemResponse> Items);
