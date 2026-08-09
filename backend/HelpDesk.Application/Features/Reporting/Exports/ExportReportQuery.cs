using MediatR;

namespace HelpDesk.Application.Features.Reporting.Exports;

public enum ReportExportFormat { Pdf, Excel }

public enum ReportType { Dashboard, MonthlyTickets, AgentPerformance, Sla, EmployeeActivity }

public sealed record ExportReportQuery(
    ReportType Type,
    ReportExportFormat Format,
    DateTime? FromUtc = null,
    DateTime? ToUtc = null,
    int Months = 12,
    string? Role = null) : IRequest<ReportFileResponse>;

public sealed record ReportFileResponse(
    byte[] Content,
    string ContentType,
    string FileName);

public sealed record ReportDocument(
    string Title,
    string Subtitle,
    DateTime GeneratedAtUtc,
    IReadOnlyList<ReportSection> Sections);

public sealed record ReportSection(
    string Title,
    IReadOnlyList<string> Columns,
    IReadOnlyList<IReadOnlyList<string>> Rows);
