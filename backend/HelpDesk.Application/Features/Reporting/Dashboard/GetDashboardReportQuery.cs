using MediatR;

namespace HelpDesk.Application.Features.Reporting.Dashboard;

public sealed record GetDashboardReportQuery(
    DateTime? FromUtc = null,
    DateTime? ToUtc = null) : IRequest<DashboardReportResponse>;
