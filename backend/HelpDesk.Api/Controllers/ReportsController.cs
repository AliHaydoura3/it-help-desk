using HelpDesk.Application.Common.Authorization;
using HelpDesk.Application.Features.Reporting.AgentPerformance;
using HelpDesk.Application.Features.Reporting.Dashboard;
using HelpDesk.Application.Features.Reporting.EmployeeActivity;
using HelpDesk.Application.Features.Reporting.Exports;
using HelpDesk.Application.Features.Reporting.Monthly;
using HelpDesk.Application.Features.Reporting.Sla;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Api.Controllers;

[ApiController]
[Authorize(Policy = Policies.Reporting)]
[Route("api/reports")]
public sealed class ReportsController(ISender sender) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardReportResponse>> GetDashboard(
        [FromQuery] GetDashboardReportQuery query,
        CancellationToken cancellationToken) =>
        Ok(await sender.Send(query, cancellationToken));

    [HttpGet("agent-performance")]
    public async Task<ActionResult<AgentPerformanceReportResponse>> GetAgentPerformance(
        [FromQuery] GetAgentPerformanceReportQuery query,
        CancellationToken cancellationToken) =>
        Ok(await sender.Send(query, cancellationToken));

    [HttpGet("monthly")]
    public async Task<ActionResult<MonthlyTicketReportResponse>> GetMonthly(
        [FromQuery] GetMonthlyTicketReportQuery query,
        CancellationToken cancellationToken) =>
        Ok(await sender.Send(query, cancellationToken));

    [HttpGet("sla")]
    public async Task<ActionResult<SlaReportResponse>> GetSla(
        [FromQuery] GetSlaReportQuery query,
        CancellationToken cancellationToken) =>
        Ok(await sender.Send(query, cancellationToken));

    [HttpGet("employee-activity")]
    public async Task<ActionResult<EmployeeActivityReportResponse>> GetEmployeeActivity(
        [FromQuery] GetEmployeeActivityReportQuery query,
        CancellationToken cancellationToken) =>
        Ok(await sender.Send(query, cancellationToken));

    [HttpGet("export")]
    public async Task<IActionResult> Export(
        [FromQuery] ExportReportQuery query,
        CancellationToken cancellationToken)
    {
        var report = await sender.Send(query, cancellationToken);
        return File(report.Content, report.ContentType, report.FileName);
    }
}
