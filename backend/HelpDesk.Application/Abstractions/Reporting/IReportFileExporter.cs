using HelpDesk.Application.Features.Reporting.Exports;

namespace HelpDesk.Application.Abstractions.Reporting;

public interface IReportFileExporter
{
    ReportExportFormat Format { get; }
    ReportFileResponse Export(ReportDocument document);
}
