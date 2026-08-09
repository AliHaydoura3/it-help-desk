using HelpDesk.Application.Abstractions.Communication;
using HelpDesk.Application.Common.Notifications;
using HelpDesk.Domain;
using HelpDesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using HelpDesk.Application.Abstractions.Admin;

namespace HelpDesk.Infrastructure.Communication;

public sealed class NotificationQueue(
    ApplicationDbContext dbContext,
    IOperationalSettingsReader settingsReader) : INotificationQueue
{
    public async Task QueueAsync(
        NotificationMessage message,
        CancellationToken cancellationToken)
    {
        var recipientIds = message.RecipientUserIds
            .Where(userId => userId != Guid.Empty && userId != message.ActorUserId)
            .Distinct()
            .ToArray();

        if (recipientIds.Length == 0) return;

        var settings = await settingsReader.GetAsync(cancellationToken);

        var activeRecipientIds = await dbContext.Users
            .Where(user => user.IsActive && recipientIds.Contains(user.Id))
            .Select(user => user.Id)
            .ToListAsync(cancellationToken);
        var now = DateTime.UtcNow;

        dbContext.Notifications.AddRange(activeRecipientIds.Select(recipientId =>
            Notification.Create(
                recipientId,
                message.ActorUserId,
                message.TicketId,
                message.Type,
                message.Title,
                message.Message,
                now,
                message.SendEmail && settings.EmailNotificationsEnabled)));
    }

    public async Task QueueToRolesAsync(
        NotificationMessage message,
        IReadOnlyCollection<string> roleNames,
        CancellationToken cancellationToken)
    {
        var normalizedRoleNames = roleNames
            .Where(role => !string.IsNullOrWhiteSpace(role))
            .Select(role => role.ToUpperInvariant())
            .Distinct()
            .ToArray();
        var roleIds = dbContext.Roles
            .Where(role => role.NormalizedName != null &&
                normalizedRoleNames.Contains(role.NormalizedName))
            .Select(role => role.Id);
        var recipientIds = await dbContext.UserRoles
            .Where(userRole => roleIds.Contains(userRole.RoleId))
            .Select(userRole => userRole.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);

        await QueueAsync(
            message with { RecipientUserIds = recipientIds },
            cancellationToken);
    }
}
