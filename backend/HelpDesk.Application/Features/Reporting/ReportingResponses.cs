using HelpDesk.Domain;

namespace HelpDesk.Application.Features.Reporting;

public sealed record ReportingPeriodResponse(DateTime FromUtc, DateTime ToUtc);

public sealed record CategoryMetricResponse(TicketCategory Category, int Count);

public sealed record PriorityMetricResponse(TicketPriority Priority, int Count);

public sealed record AgentPerformanceItemResponse(
    Guid AgentId,
    string FirstName,
    string LastName,
    string Email,
    int ActiveAssignedTickets,
    int PendingTickets,
    int ResolvedTickets,
    double? AverageResolutionHours,
    double? SlaCompliancePercentage);

public sealed record SlaSummaryResponse(
    int EvaluatedTickets,
    int CompliantTickets,
    int BreachedTickets,
    int ActiveAtRiskTickets,
    int ActiveBreachedTickets,
    double? CompliancePercentage);
