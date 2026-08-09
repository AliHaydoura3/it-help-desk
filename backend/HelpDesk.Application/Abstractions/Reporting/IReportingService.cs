using HelpDesk.Application.Features.Reporting;
using HelpDesk.Application.Features.Reporting.AgentPerformance;
using HelpDesk.Application.Features.Reporting.Dashboard;
using HelpDesk.Application.Features.Reporting.EmployeeActivity;
using HelpDesk.Application.Features.Reporting.Monthly;
using HelpDesk.Application.Features.Reporting.Sla;

namespace HelpDesk.Application.Abstractions.Reporting;

public interface IReportingService
{
    Task<DashboardReportResponse> GetDashboardAsync(GetDashboardReportQuery query, CancellationToken cancellationToken);
    Task<AgentPerformanceReportResponse> GetAgentPerformanceAsync(GetAgentPerformanceReportQuery query, CancellationToken cancellationToken);
    Task<MonthlyTicketReportResponse> GetMonthlyReportAsync(GetMonthlyTicketReportQuery query, CancellationToken cancellationToken);
    Task<SlaReportResponse> GetSlaReportAsync(GetSlaReportQuery query, CancellationToken cancellationToken);
    Task<EmployeeActivityReportResponse> GetEmployeeActivityAsync(GetEmployeeActivityReportQuery query, CancellationToken cancellationToken);
}
