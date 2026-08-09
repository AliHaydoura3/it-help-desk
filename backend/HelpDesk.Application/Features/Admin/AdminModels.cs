using HelpDesk.Domain;

namespace HelpDesk.Application.Features.Admin;

public sealed record AdminDashboardResponse(
    AdminUserMetrics Users,
    AdminTicketMetrics Tickets,
    AdminNotificationMetrics Notifications,
    AdminStorageMetrics Storage,
    AdminAuditMetrics Audit,
    DateTime GeneratedAtUtc);

public sealed record AdminUserMetrics(
    int Total,
    int Active,
    int Inactive,
    int SupportAgents);

public sealed record AdminTicketMetrics(
    int Total,
    int Open,
    int InProgress,
    int Pending,
    int Resolved,
    int Closed,
    int Critical,
    int Unassigned);

public sealed record AdminNotificationMetrics(
    int Total,
    int PendingEmail,
    int FailedEmail,
    int Unread);

public sealed record AdminStorageMetrics(int AttachmentCount, long AttachmentBytes);
public sealed record AdminAuditMetrics(int RequestsLast24Hours, int FailedRequestsLast24Hours);

public sealed record AdminRoleResponse(
    string Name,
    string DisplayName,
    string Description,
    int AssignedUserCount,
    IReadOnlyList<string> Permissions);

public sealed record TicketCategorySettingResponse(
    TicketCategory Category,
    string DisplayName,
    string Description,
    bool IsActive,
    int SortOrder,
    DateTime UpdatedAtUtc,
    Guid? UpdatedByUserId);

public sealed record SystemSettingsResponse(
    string OrganizationName,
    string SupportEmail,
    bool AutomaticAssignmentEnabled,
    bool EmailNotificationsEnabled,
    int MaximumOpenTicketsPerEmployee,
    DateTime UpdatedAtUtc,
    Guid? UpdatedByUserId);
