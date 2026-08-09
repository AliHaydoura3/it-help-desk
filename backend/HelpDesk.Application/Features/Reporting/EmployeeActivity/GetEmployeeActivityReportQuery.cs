using MediatR;

namespace HelpDesk.Application.Features.Reporting.EmployeeActivity;

public sealed record GetEmployeeActivityReportQuery(
    DateTime? FromUtc = null,
    DateTime? ToUtc = null,
    string? Role = null,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<EmployeeActivityReportResponse>;

public sealed record EmployeeActivityItemResponse(
    Guid UserId,
    string FirstName,
    string LastName,
    string Email,
    string Role,
    bool IsActive,
    int TicketsCreated,
    int TicketsResolved,
    int CommentsAdded,
    int SuccessfulActions,
    int FailedActions,
    DateTime? LastActivityAtUtc);

public sealed record EmployeeActivityReportResponse(
    DateTime GeneratedAtUtc,
    ReportingPeriodResponse Period,
    IReadOnlyList<EmployeeActivityItemResponse> Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages);
