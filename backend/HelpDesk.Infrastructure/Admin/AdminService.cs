using HelpDesk.Application.Abstractions.Admin;
using HelpDesk.Application.Abstractions.Authentication;
using HelpDesk.Application.Common.Authorization;
using HelpDesk.Application.Features.Admin;
using HelpDesk.Domain;
using HelpDesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infrastructure.Admin;

public sealed class AdminService(ApplicationDbContext dbContext, ICurrentUser currentUser)
    : IAdminService
{
    public async Task<AdminDashboardResponse> GetDashboardAsync(CancellationToken cancellationToken)
    {
        var activeTicketQuery = dbContext.Tickets.AsNoTracking().Where(ticket => !ticket.IsCancelled);
        var supportRoleId = dbContext.Roles
            .Where(role => role.NormalizedName == Roles.ITSupportSpecialist.ToUpperInvariant())
            .Select(role => role.Id);
        var supportUserIds = dbContext.UserRoles
            .Where(userRole => supportRoleId.Contains(userRole.RoleId))
            .Select(userRole => userRole.UserId);

        var totalUsers = await dbContext.Users.CountAsync(cancellationToken);
        var activeUsers = await dbContext.Users.CountAsync(user => user.IsActive, cancellationToken);
        var supportAgents = await dbContext.Users.CountAsync(
            user => user.IsActive && supportUserIds.Contains(user.Id), cancellationToken);
        var totalTickets = await activeTicketQuery.CountAsync(cancellationToken);
        var open = await activeTicketQuery.CountAsync(ticket => ticket.Status == TicketStatus.Open, cancellationToken);
        var inProgress = await activeTicketQuery.CountAsync(ticket => ticket.Status == TicketStatus.InProgress, cancellationToken);
        var pending = await activeTicketQuery.CountAsync(ticket => ticket.Status == TicketStatus.Pending, cancellationToken);
        var resolved = await activeTicketQuery.CountAsync(ticket => ticket.Status == TicketStatus.Resolved, cancellationToken);
        var closed = await activeTicketQuery.CountAsync(ticket => ticket.Status == TicketStatus.Closed, cancellationToken);
        var critical = await activeTicketQuery.CountAsync(ticket => ticket.Priority == TicketPriority.Critical, cancellationToken);
        var unassigned = await activeTicketQuery.CountAsync(ticket => ticket.AssignedToUserId == null && ticket.Status != TicketStatus.Closed, cancellationToken);
        var totalNotifications = await dbContext.Notifications.CountAsync(cancellationToken);
        var pendingEmail = await dbContext.Notifications.CountAsync(notification => notification.EmailStatus == NotificationEmailStatus.Pending, cancellationToken);
        var failedEmail = await dbContext.Notifications.CountAsync(notification => notification.EmailStatus == NotificationEmailStatus.Failed, cancellationToken);
        var unread = await dbContext.Notifications.CountAsync(notification => !notification.IsRead, cancellationToken);
        var attachmentCount = await dbContext.TicketAttachments.CountAsync(cancellationToken);
        var attachmentBytes = await dbContext.TicketAttachments.SumAsync(attachment => (long?)attachment.SizeBytes, cancellationToken) ?? 0;
        var since = DateTime.UtcNow.AddHours(-24);
        var requests = await dbContext.UserActivityLogs.CountAsync(log => log.OccurredAtUtc >= since, cancellationToken);
        var failedRequests = await dbContext.UserActivityLogs.CountAsync(log => log.OccurredAtUtc >= since && !log.Succeeded, cancellationToken);

        return new AdminDashboardResponse(
            new AdminUserMetrics(totalUsers, activeUsers, totalUsers - activeUsers, supportAgents),
            new AdminTicketMetrics(totalTickets, open, inProgress, pending, resolved, closed, critical, unassigned),
            new AdminNotificationMetrics(totalNotifications, pendingEmail, failedEmail, unread),
            new AdminStorageMetrics(attachmentCount, attachmentBytes),
            new AdminAuditMetrics(requests, failedRequests),
            DateTime.UtcNow);
    }

    public async Task<IReadOnlyList<AdminRoleResponse>> GetRolesAsync(CancellationToken cancellationToken)
    {
        var counts = await (from userRole in dbContext.UserRoles
                            join role in dbContext.Roles on userRole.RoleId equals role.Id
                            group userRole by role.Name into grouping
                            select new { Name = grouping.Key!, Count = grouping.Count() })
            .ToDictionaryAsync(item => item.Name, item => item.Count, StringComparer.OrdinalIgnoreCase, cancellationToken);

        return Roles.All.Select(role => new AdminRoleResponse(
            role,
            GetRoleDisplayName(role),
            GetRoleDescription(role),
            counts.GetValueOrDefault(role),
            RolePermissions.GetPermissions(role).OrderBy(permission => permission.ToString()).Select(permission => permission.ToString()).ToArray()))
            .ToArray();
    }

    public async Task<IReadOnlyList<TicketCategorySettingResponse>> GetCategoriesAsync(bool activeOnly, CancellationToken cancellationToken)
    {
        var query = dbContext.TicketCategorySettings.AsNoTracking();
        if (activeOnly) query = query.Where(setting => setting.IsActive);
        return await query.OrderBy(setting => setting.SortOrder)
            .ThenBy(setting => setting.DisplayName)
            .Select(setting => MapCategory(setting))
            .ToListAsync(cancellationToken);
    }

    public async Task<TicketCategorySettingResponse> UpdateCategoryAsync(
        TicketCategory category,
        UpdateTicketCategoryCommand command,
        CancellationToken cancellationToken)
    {
        var setting = await dbContext.TicketCategorySettings.SingleOrDefaultAsync(
            item => item.Category == category, cancellationToken)
            ?? throw new KeyNotFoundException($"Ticket category '{category}' does not exist.");
        setting.Update(currentUser.UserId, command.DisplayName, command.Description,
            command.IsActive, command.SortOrder, DateTime.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);
        return MapCategory(setting);
    }

    public async Task<SystemSettingsResponse> GetSettingsAsync(CancellationToken cancellationToken)
    {
        var settings = await FindSettingsAsync(cancellationToken);
        return MapSettings(settings);
    }

    public async Task<SystemSettingsResponse> UpdateSettingsAsync(UpdateSystemSettingsCommand command, CancellationToken cancellationToken)
    {
        var settings = await FindSettingsAsync(cancellationToken);
        settings.Update(currentUser.UserId, command.OrganizationName, command.SupportEmail,
            command.AutomaticAssignmentEnabled, command.EmailNotificationsEnabled,
            command.MaximumOpenTicketsPerEmployee, DateTime.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);
        return MapSettings(settings);
    }

    private async Task<SystemSettings> FindSettingsAsync(CancellationToken cancellationToken)
    {
        var settings = await dbContext.SystemSettings.SingleOrDefaultAsync(
            item => item.Id == SystemSettings.SingletonId, cancellationToken);
        if (settings is not null) return settings;
        settings = SystemSettings.CreateDefaults(DateTime.UtcNow);
        dbContext.SystemSettings.Add(settings);
        return settings;
    }

    private static TicketCategorySettingResponse MapCategory(TicketCategorySetting setting) =>
        new(setting.Category, setting.DisplayName, setting.Description, setting.IsActive,
            setting.SortOrder, setting.UpdatedAtUtc, setting.UpdatedByUserId);

    private static SystemSettingsResponse MapSettings(SystemSettings settings) =>
        new(settings.OrganizationName, settings.SupportEmail, settings.AutomaticAssignmentEnabled,
            settings.EmailNotificationsEnabled, settings.MaximumOpenTicketsPerEmployee,
            settings.UpdatedAtUtc, settings.UpdatedByUserId);

    private static string GetRoleDisplayName(string role) => role switch
    {
        Roles.ITSupportSpecialist => "IT support agent",
        Roles.Admin => "Administrator",
        _ => role
    };

    private static string GetRoleDescription(string role) => role switch
    {
        Roles.Admin => "Full system governance, configuration, reporting, and operational access.",
        Roles.ITSupportSpecialist => "Manage the ticket queue, assignment workflow, resolution, notes, and replies.",
        Roles.Manager => "Monitor tickets and assignment history, and analyze operational reports.",
        _ => "Create, update, cancel, comment on, and track their own support tickets."
    };
}
