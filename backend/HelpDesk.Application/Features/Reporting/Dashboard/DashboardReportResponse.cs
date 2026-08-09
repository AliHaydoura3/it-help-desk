namespace HelpDesk.Application.Features.Reporting.Dashboard;

public sealed record DashboardReportResponse(
    DateTime GeneratedAtUtc,
    ReportingPeriodResponse PerformancePeriod,
    int TotalTickets,
    int OpenTickets,
    int InProgressTickets,
    int PendingTickets,
    int ResolvedTickets,
    int ClosedTickets,
    IReadOnlyList<CategoryMetricResponse> TicketsByCategory,
    IReadOnlyList<PriorityMetricResponse> TicketsByPriority,
    IReadOnlyList<AgentPerformanceItemResponse> AgentPerformance,
    double? AverageResolutionHours,
    SlaSummaryResponse Sla);
