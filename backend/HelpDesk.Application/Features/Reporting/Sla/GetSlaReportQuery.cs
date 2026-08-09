using HelpDesk.Domain;
using MediatR;

namespace HelpDesk.Application.Features.Reporting.Sla;

public sealed record GetSlaReportQuery(
    DateTime? FromUtc = null,
    DateTime? ToUtc = null) : IRequest<SlaReportResponse>;

public sealed record SlaPriorityMetricResponse(
    TicketPriority Priority,
    double TargetHours,
    int EvaluatedTickets,
    int CompliantTickets,
    int BreachedTickets,
    int ActiveAtRiskTickets,
    int ActiveBreachedTickets,
    double? CompliancePercentage,
    double? AverageResolutionHours);

public sealed record SlaReportResponse(
    DateTime GeneratedAtUtc,
    ReportingPeriodResponse Period,
    SlaSummaryResponse Summary,
    IReadOnlyList<SlaPriorityMetricResponse> ByPriority);
