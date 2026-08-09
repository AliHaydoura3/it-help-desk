using MediatR;

namespace HelpDesk.Application.Features.Reporting.Monthly;

public sealed record GetMonthlyTicketReportQuery(int Months = 12)
    : IRequest<MonthlyTicketReportResponse>;

public sealed record MonthlyTicketMetricResponse(
    int Year,
    int Month,
    string Label,
    int CreatedTickets,
    int ResolvedTickets,
    int ClosedTickets,
    int CancelledTickets,
    double? AverageResolutionHours);

public sealed record MonthlyTicketReportResponse(
    DateTime GeneratedAtUtc,
    ReportingPeriodResponse Period,
    int TotalCreatedTickets,
    int TotalResolvedTickets,
    int TotalClosedTickets,
    int TotalCancelledTickets,
    double? AverageResolutionHours,
    IReadOnlyList<MonthlyTicketMetricResponse> Months);
