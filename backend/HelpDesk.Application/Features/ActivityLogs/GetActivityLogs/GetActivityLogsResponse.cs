namespace HelpDesk.Application.Features.ActivityLogs.GetActivityLogs;

public sealed record ActivityLogResponse(
    Guid Id,
    Guid? UserId,
    string? UserEmail,
    string Action,
    string Resource,
    string? ResourceId,
    string? IpAddress,
    bool Succeeded,
    DateTime OccurredAtUtc);

public sealed record GetActivityLogsResponse(
    IReadOnlyList<ActivityLogResponse> Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages);
