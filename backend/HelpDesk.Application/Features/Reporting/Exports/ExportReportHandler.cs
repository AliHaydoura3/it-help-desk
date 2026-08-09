using HelpDesk.Application.Abstractions.Reporting;
using HelpDesk.Application.Features.Reporting.AgentPerformance;
using HelpDesk.Application.Features.Reporting.Dashboard;
using HelpDesk.Application.Features.Reporting.EmployeeActivity;
using HelpDesk.Application.Features.Reporting.Monthly;
using HelpDesk.Application.Features.Reporting.Sla;
using MediatR;

namespace HelpDesk.Application.Features.Reporting.Exports;

public sealed class ExportReportHandler(
    IReportingService reportingService,
    IEnumerable<IReportFileExporter> exporters)
    : IRequestHandler<ExportReportQuery, ReportFileResponse>
{
    public async Task<ReportFileResponse> Handle(
        ExportReportQuery request,
        CancellationToken cancellationToken)
    {
        var exporter = exporters.SingleOrDefault(item => item.Format == request.Format)
            ?? throw new InvalidOperationException(
                $"No exporter is registered for '{request.Format}'.");
        var document = request.Type switch
        {
            ReportType.Dashboard => ReportDocumentFactory.Create(
                await reportingService.GetDashboardAsync(
                    new GetDashboardReportQuery(request.FromUtc, request.ToUtc),
                    cancellationToken)),
            ReportType.MonthlyTickets => ReportDocumentFactory.Create(
                await reportingService.GetMonthlyReportAsync(
                    new GetMonthlyTicketReportQuery(request.Months),
                    cancellationToken)),
            ReportType.AgentPerformance => ReportDocumentFactory.Create(
                await reportingService.GetAgentPerformanceAsync(
                    new GetAgentPerformanceReportQuery(request.FromUtc, request.ToUtc),
                    cancellationToken)),
            ReportType.Sla => ReportDocumentFactory.Create(
                await reportingService.GetSlaReportAsync(
                    new GetSlaReportQuery(request.FromUtc, request.ToUtc),
                    cancellationToken)),
            ReportType.EmployeeActivity => ReportDocumentFactory.Create(
                await GetAllEmployeeActivityAsync(request, cancellationToken)),
            _ => throw new ArgumentOutOfRangeException(
                nameof(request.Type), request.Type, null)
        };
        return exporter.Export(document);
    }

    private async Task<EmployeeActivityReportResponse> GetAllEmployeeActivityAsync(
        ExportReportQuery request,
        CancellationToken cancellationToken)
    {
        const int exportPageSize = 100;
        var firstPage = await reportingService.GetEmployeeActivityAsync(
            new GetEmployeeActivityReportQuery(
                request.FromUtc,
                request.ToUtc,
                request.Role,
                1,
                exportPageSize),
            cancellationToken);
        if (firstPage.TotalPages <= 1) return firstPage;

        var items = firstPage.Items.ToList();
        for (var page = 2; page <= firstPage.TotalPages; page++)
        {
            var nextPage = await reportingService.GetEmployeeActivityAsync(
                new GetEmployeeActivityReportQuery(
                    request.FromUtc,
                    request.ToUtc,
                    request.Role,
                    page,
                    exportPageSize),
                cancellationToken);
            items.AddRange(nextPage.Items);
        }

        return firstPage with
        {
            Items = items,
            PageSize = items.Count,
            TotalPages = items.Count == 0 ? 0 : 1
        };
    }
}
