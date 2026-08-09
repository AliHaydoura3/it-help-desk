using HelpDesk.Application.Abstractions.Reporting;
using MediatR;

namespace HelpDesk.Application.Features.Reporting.EmployeeActivity;

public sealed class GetEmployeeActivityReportHandler(IReportingService reportingService)
    : IRequestHandler<GetEmployeeActivityReportQuery, EmployeeActivityReportResponse>
{
    public Task<EmployeeActivityReportResponse> Handle(
        GetEmployeeActivityReportQuery request,
        CancellationToken cancellationToken) =>
        reportingService.GetEmployeeActivityAsync(request, cancellationToken);
}
