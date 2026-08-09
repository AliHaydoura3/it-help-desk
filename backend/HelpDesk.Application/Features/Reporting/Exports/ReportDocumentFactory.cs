using System.Globalization;
using HelpDesk.Application.Features.Reporting.AgentPerformance;
using HelpDesk.Application.Features.Reporting.Dashboard;
using HelpDesk.Application.Features.Reporting.EmployeeActivity;
using HelpDesk.Application.Features.Reporting.Monthly;
using HelpDesk.Application.Features.Reporting.Sla;

namespace HelpDesk.Application.Features.Reporting.Exports;

public static class ReportDocumentFactory
{
    public static ReportDocument Create(DashboardReportResponse report) =>
        new(
            "Help Desk Dashboard",
            PeriodSubtitle(report.PerformancePeriod),
            report.GeneratedAtUtc,
            [
                new ReportSection("Ticket status", ["Metric", "Count"],
                [
                    ["Total", Number(report.TotalTickets)],
                    ["Open", Number(report.OpenTickets)],
                    ["In progress", Number(report.InProgressTickets)],
                    ["Pending", Number(report.PendingTickets)],
                    ["Resolved", Number(report.ResolvedTickets)],
                    ["Closed", Number(report.ClosedTickets)],
                    ["Average resolution hours", Number(report.AverageResolutionHours)]
                ]),
                new ReportSection("Tickets by category", ["Category", "Count"],
                    report.TicketsByCategory.Select(item => (IReadOnlyList<string>)
                        [item.Category.ToString(), Number(item.Count)]).ToList()),
                new ReportSection("Tickets by priority", ["Priority", "Count"],
                    report.TicketsByPriority.Select(item => (IReadOnlyList<string>)
                        [item.Priority.ToString(), Number(item.Count)]).ToList()),
                AgentSection(report.AgentPerformance),
                SlaSummarySection(report.Sla)
            ]);

    public static ReportDocument Create(MonthlyTicketReportResponse report) =>
        new(
            "Monthly Ticket Report",
            PeriodSubtitle(report.Period),
            report.GeneratedAtUtc,
            [new ReportSection(
                "Monthly totals",
                ["Month", "Created", "Resolved", "Closed", "Cancelled", "Avg resolution (h)"],
                report.Months.Select(month => (IReadOnlyList<string>)
                [
                    month.Label,
                    Number(month.CreatedTickets),
                    Number(month.ResolvedTickets),
                    Number(month.ClosedTickets),
                    Number(month.CancelledTickets),
                    Number(month.AverageResolutionHours)
                ]).ToList())]);

    public static ReportDocument Create(AgentPerformanceReportResponse report) =>
        new(
            "Agent Performance Report",
            PeriodSubtitle(report.Period),
            report.GeneratedAtUtc,
            [AgentSection(report.Items)]);

    public static ReportDocument Create(SlaReportResponse report) =>
        new(
            "SLA Report",
            PeriodSubtitle(report.Period),
            report.GeneratedAtUtc,
            [
                SlaSummarySection(report.Summary),
                new ReportSection(
                    "SLA by priority",
                    ["Priority", "Target (h)", "Evaluated", "Compliant", "Breached", "At risk", "Active breached", "Compliance %", "Avg resolution (h)"],
                    report.ByPriority.Select(metric => (IReadOnlyList<string>)
                    [
                        metric.Priority.ToString(),
                        Number(metric.TargetHours),
                        Number(metric.EvaluatedTickets),
                        Number(metric.CompliantTickets),
                        Number(metric.BreachedTickets),
                        Number(metric.ActiveAtRiskTickets),
                        Number(metric.ActiveBreachedTickets),
                        Number(metric.CompliancePercentage),
                        Number(metric.AverageResolutionHours)
                    ]).ToList())
            ]);

    public static ReportDocument Create(EmployeeActivityReportResponse report) =>
        new(
            "Employee Activity Report",
            PeriodSubtitle(report.Period),
            report.GeneratedAtUtc,
            [new ReportSection(
                "User activity",
                ["Employee", "Email", "Role", "Active", "Tickets created", "Tickets resolved", "Comments", "Successful actions", "Failed actions", "Last activity (UTC)"],
                report.Items.Select(item => (IReadOnlyList<string>)
                [
                    $"{item.FirstName} {item.LastName}",
                    item.Email,
                    item.Role,
                    item.IsActive ? "Yes" : "No",
                    Number(item.TicketsCreated),
                    Number(item.TicketsResolved),
                    Number(item.CommentsAdded),
                    Number(item.SuccessfulActions),
                    Number(item.FailedActions),
                    item.LastActivityAtUtc?.ToString("u", CultureInfo.InvariantCulture) ?? "—"
                ]).ToList())]);

    private static ReportSection AgentSection(IReadOnlyList<AgentPerformanceItemResponse> agents) =>
        new(
            "Agent performance",
            ["Agent", "Email", "Active", "Pending", "Resolved", "Avg resolution (h)", "SLA compliance %"],
            agents.Select(agent => (IReadOnlyList<string>)
            [
                $"{agent.FirstName} {agent.LastName}",
                agent.Email,
                Number(agent.ActiveAssignedTickets),
                Number(agent.PendingTickets),
                Number(agent.ResolvedTickets),
                Number(agent.AverageResolutionHours),
                Number(agent.SlaCompliancePercentage)
            ]).ToList());

    private static ReportSection SlaSummarySection(SlaSummaryResponse summary) =>
        new(
            "SLA summary",
            ["Metric", "Value"],
            [
                ["Evaluated", Number(summary.EvaluatedTickets)],
                ["Compliant", Number(summary.CompliantTickets)],
                ["Resolved breaches", Number(summary.BreachedTickets)],
                ["Active at risk", Number(summary.ActiveAtRiskTickets)],
                ["Active breached", Number(summary.ActiveBreachedTickets)],
                ["Compliance %", Number(summary.CompliancePercentage)]
            ]);

    private static string Number(int value) =>
        value.ToString(CultureInfo.InvariantCulture);

    private static string Number(double value) =>
        value.ToString("0.##", CultureInfo.InvariantCulture);

    private static string Number(double? value) =>
        value?.ToString("0.##", CultureInfo.InvariantCulture) ?? "—";

    private static string PeriodSubtitle(ReportingPeriodResponse period) =>
        $"{period.FromUtc:u} to {period.ToUtc:u}";
}
