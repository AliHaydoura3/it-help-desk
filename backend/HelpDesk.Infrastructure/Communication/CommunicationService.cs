using FluentValidation;
using FluentValidation.Results;
using HelpDesk.Application.Abstractions.Authentication;
using HelpDesk.Application.Abstractions.Communication;
using HelpDesk.Application.Common.Authorization;
using HelpDesk.Application.Common.Exceptions;
using HelpDesk.Application.Common.Notifications;
using HelpDesk.Application.Features.Communication;
using HelpDesk.Application.Features.Communication.Comments;
using HelpDesk.Application.Features.Communication.Mentions;
using HelpDesk.Application.Features.Communication.Notifications;
using HelpDesk.Domain;
using HelpDesk.Infrastructure.Identity;
using HelpDesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infrastructure.Communication;

public sealed class CommunicationService(
    ApplicationDbContext dbContext,
    ICurrentUser currentUser,
    INotificationQueue notificationQueue) : ICommunicationService
{
    public async Task<TicketCommentResponse> AddCommentAsync(
        AddTicketCommentCommand command,
        CancellationToken cancellationToken)
    {
        var ticket = await FindTicketAsync(command.TicketId, cancellationToken);
        EnsureCanRead(ticket);
        EnsureCanComment(ticket);

        TicketComment? parentComment = null;
        if (command.ParentCommentId.HasValue)
        {
            parentComment = await dbContext.TicketComments.AsNoTracking()
                .SingleOrDefaultAsync(
                    comment => comment.Id == command.ParentCommentId.Value &&
                        comment.TicketId == ticket.Id,
                    cancellationToken)
                ?? throw new KeyNotFoundException(
                    $"Comment '{command.ParentCommentId}' does not exist on this ticket.");
        }

        var mentionedAgentIds = (command.MentionedAgentIds ?? [])
            .Where(userId => userId != currentUser.UserId)
            .Distinct()
            .ToArray();
        await EnsureMentionedAgentsAreValidAsync(mentionedAgentIds, cancellationToken);

        var now = DateTime.UtcNow;
        var comment = TicketComment.Create(
            ticket.Id,
            currentUser.UserId,
            parentComment?.Id,
            command.Content,
            mentionedAgentIds,
            now);
        var history = ticket.RecordComment(
            currentUser.UserId,
            comment.Id,
            parentComment is not null,
            now);

        dbContext.TicketComments.Add(comment);
        dbContext.TicketHistories.Add(history);

        var standardRecipients = BuildTicketParticipantIds(ticket)
            .Append(parentComment?.AuthorUserId ?? Guid.Empty)
            .Except(mentionedAgentIds)
            .ToArray();
        var action = parentComment is null ? "commented on" : "replied on";

        await notificationQueue.QueueAsync(
            new NotificationMessage(
                currentUser.UserId,
                ticket.Id,
                parentComment is null ? NotificationType.CommentAdded : NotificationType.ReplyAdded,
                parentComment is null ? "New ticket comment" : "New ticket reply",
                $"A user {action} ticket {ticket.ReferenceNumber}.",
                standardRecipients),
            cancellationToken);

        if (mentionedAgentIds.Length > 0)
        {
            await notificationQueue.QueueAsync(
                new NotificationMessage(
                    currentUser.UserId,
                    ticket.Id,
                    NotificationType.AgentMentioned,
                    "You were mentioned",
                    $"You were mentioned in a comment on ticket {ticket.ReferenceNumber}.",
                    mentionedAgentIds),
                cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return await MapCommentAsync(comment, cancellationToken);
    }

    public async Task<GetTicketCommentsResponse> GetCommentsAsync(
        GetTicketCommentsQuery query,
        CancellationToken cancellationToken)
    {
        var ticket = await dbContext.Tickets.AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == query.TicketId, cancellationToken)
            ?? throw new KeyNotFoundException($"Ticket '{query.TicketId}' does not exist.");
        EnsureCanRead(ticket);

        var commentsQuery = dbContext.TicketComments.AsNoTracking()
            .Where(comment => comment.TicketId == query.TicketId);
        var totalCount = await commentsQuery.CountAsync(cancellationToken);
        var comments = await commentsQuery
            .Include(comment => comment.Mentions)
            .OrderBy(comment => comment.CreatedAtUtc)
            .ThenBy(comment => comment.Id)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);
        var responses = await MapCommentsAsync(comments, cancellationToken);

        return new GetTicketCommentsResponse(
            responses,
            query.PageNumber,
            query.PageSize,
            totalCount,
            (int)Math.Ceiling(totalCount / (double)query.PageSize));
    }

    public async Task<IReadOnlyList<MentionableAgentResponse>> GetMentionableAgentsAsync(
        GetMentionableAgentsQuery query,
        CancellationToken cancellationToken)
    {
        var ticket = await dbContext.Tickets.AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == query.TicketId, cancellationToken)
            ?? throw new KeyNotFoundException($"Ticket '{query.TicketId}' does not exist.");
        EnsureCanRead(ticket);

        var agents = BuildActiveSupportAgentsQuery();
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            agents = agents.Where(user =>
                user.FirstName.Contains(search) ||
                user.LastName.Contains(search) ||
                (user.FirstName + " " + user.LastName).Contains(search) ||
                (user.Email != null && user.Email.Contains(search)));
        }

        return await agents
            .OrderBy(user => user.FirstName)
            .ThenBy(user => user.LastName)
            .Take(query.Limit)
            .Select(user => new MentionableAgentResponse(
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email!))
            .ToListAsync(cancellationToken);
    }

    public async Task<GetNotificationsResponse> GetNotificationsAsync(
        GetNotificationsQuery query,
        CancellationToken cancellationToken)
    {
        var allNotifications = dbContext.Notifications.AsNoTracking()
            .Where(notification => notification.RecipientUserId == currentUser.UserId);
        var unreadCount = await allNotifications.CountAsync(
            notification => !notification.IsRead,
            cancellationToken);
        var filteredNotifications = query.IsRead.HasValue
            ? allNotifications.Where(notification => notification.IsRead == query.IsRead.Value)
            : allNotifications;
        var totalCount = await filteredNotifications.CountAsync(cancellationToken);
        var notifications = await filteredNotifications
            .OrderByDescending(notification => notification.CreatedAtUtc)
            .ThenByDescending(notification => notification.Id)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Join(
                dbContext.Users.AsNoTracking(),
                notification => notification.ActorUserId,
                actor => actor.Id,
                (notification, actor) => new NotificationResponse(
                    notification.Id,
                    new NotificationActorResponse(
                        actor.Id,
                        actor.FirstName,
                        actor.LastName),
                    notification.TicketId,
                    notification.Type,
                    notification.Title,
                    notification.Message,
                    notification.IsRead,
                    notification.CreatedAtUtc,
                    notification.ReadAtUtc))
            .ToListAsync(cancellationToken);

        return new GetNotificationsResponse(
            notifications,
            query.PageNumber,
            query.PageSize,
            totalCount,
            (int)Math.Ceiling(totalCount / (double)query.PageSize),
            unreadCount);
    }

    public async Task<UnreadNotificationCountResponse> GetUnreadCountAsync(
        GetUnreadNotificationCountQuery query,
        CancellationToken cancellationToken)
    {
        var count = await dbContext.Notifications.AsNoTracking().CountAsync(
            notification => notification.RecipientUserId == currentUser.UserId &&
                !notification.IsRead,
            cancellationToken);
        return new UnreadNotificationCountResponse(count);
    }

    public async Task<NotificationResponse> MarkNotificationReadAsync(
        MarkNotificationReadCommand command,
        CancellationToken cancellationToken)
    {
        var notification = await dbContext.Notifications.SingleOrDefaultAsync(
            item => item.Id == command.NotificationId &&
                item.RecipientUserId == currentUser.UserId,
            cancellationToken)
            ?? throw new KeyNotFoundException(
                $"Notification '{command.NotificationId}' does not exist.");

        notification.MarkRead(DateTime.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);
        return await MapNotificationAsync(notification, cancellationToken);
    }

    public async Task<MarkAllNotificationsReadResponse> MarkAllNotificationsReadAsync(
        MarkAllNotificationsReadCommand command,
        CancellationToken cancellationToken)
    {
        var notifications = await dbContext.Notifications
            .Where(notification => notification.RecipientUserId == currentUser.UserId &&
                !notification.IsRead)
            .ToListAsync(cancellationToken);
        var now = DateTime.UtcNow;

        foreach (var notification in notifications)
            notification.MarkRead(now);

        await dbContext.SaveChangesAsync(cancellationToken);
        return new MarkAllNotificationsReadResponse(notifications.Count);
    }

    private async Task<Ticket> FindTicketAsync(Guid ticketId, CancellationToken cancellationToken) =>
        await dbContext.Tickets.SingleOrDefaultAsync(
            ticket => ticket.Id == ticketId,
            cancellationToken)
        ?? throw new KeyNotFoundException($"Ticket '{ticketId}' does not exist.");

    private void EnsureCanRead(Ticket ticket)
    {
        if (!currentUser.CanReadTicket(ticket))
        {
            throw new ForbiddenAccessException("You cannot access this ticket conversation.");
        }
    }

    private void EnsureCanComment(Ticket ticket)
    {
        if (ticket.IsCancelled)
            throw new InvalidOperationException(
                "A cancelled ticket cannot receive new comments.");

        if (ticket.Status == TicketStatus.Closed)
            throw new InvalidOperationException(
                "A closed ticket cannot receive new comments.");

        if (!currentUser.CanCommentOnTicket(ticket))
        {
            throw new ForbiddenAccessException(
                "Your role has read-only access to this ticket conversation.");
        }
    }

    private IQueryable<ApplicationUser> BuildActiveSupportAgentsQuery()
    {
        var supportRoleIds = dbContext.Roles
            .Where(role => role.NormalizedName == Roles.ITSupportSpecialist.ToUpperInvariant())
            .Select(role => role.Id);
        var supportUserIds = dbContext.UserRoles
            .Where(userRole => supportRoleIds.Contains(userRole.RoleId))
            .Select(userRole => userRole.UserId);

        return dbContext.Users.AsNoTracking()
            .Where(user => user.IsActive && supportUserIds.Contains(user.Id));
    }

    private async Task EnsureMentionedAgentsAreValidAsync(
        IReadOnlyCollection<Guid> agentIds,
        CancellationToken cancellationToken)
    {
        if (agentIds.Count == 0) return;

        var validAgentCount = await BuildActiveSupportAgentsQuery()
            .CountAsync(user => agentIds.Contains(user.Id), cancellationToken);

        if (validAgentCount != agentIds.Count)
        {
            throw new ValidationException(
            [
                new ValidationFailure(
                    nameof(AddTicketCommentCommand.MentionedAgentIds),
                    "Every mentioned user must be an active IT Support Agent.")
            ]);
        }
    }

    private static IEnumerable<Guid> BuildTicketParticipantIds(Ticket ticket)
    {
        yield return ticket.CreatedByUserId;
        if (ticket.AssignedToUserId.HasValue)
            yield return ticket.AssignedToUserId.Value;
    }

    private async Task<TicketCommentResponse> MapCommentAsync(
        TicketComment comment,
        CancellationToken cancellationToken)
    {
        var responses = await MapCommentsAsync([comment], cancellationToken);
        return responses[0];
    }

    private async Task<IReadOnlyList<TicketCommentResponse>> MapCommentsAsync(
        IReadOnlyCollection<TicketComment> comments,
        CancellationToken cancellationToken)
    {
        if (comments.Count == 0) return [];

        var userIds = comments
            .Select(comment => comment.AuthorUserId)
            .Concat(comments.SelectMany(comment => comment.Mentions)
                .Select(mention => mention.MentionedUserId))
            .Distinct()
            .ToArray();
        var users = await dbContext.Users.AsNoTracking()
            .Where(user => userIds.Contains(user.Id))
            .ToDictionaryAsync(user => user.Id, cancellationToken);

        return comments.Select(comment =>
        {
            var author = users[comment.AuthorUserId];
            var mentions = comment.Mentions.Select(mention =>
            {
                var user = users[mention.MentionedUserId];
                return new MentionedAgentResponse(
                    user.Id,
                    user.FirstName,
                    user.LastName,
                    user.Email!);
            }).ToList();

            return new TicketCommentResponse(
                comment.Id,
                comment.TicketId,
                comment.ParentCommentId,
                comment.Content,
                new CommentAuthorResponse(
                    author.Id,
                    author.FirstName,
                    author.LastName,
                    author.Email!),
                mentions,
                comment.CreatedAtUtc);
        }).ToList();
    }

    private async Task<NotificationResponse> MapNotificationAsync(
        Notification notification,
        CancellationToken cancellationToken)
    {
        var actor = await dbContext.Users.AsNoTracking()
            .SingleAsync(user => user.Id == notification.ActorUserId, cancellationToken);

        return new NotificationResponse(
            notification.Id,
            new NotificationActorResponse(actor.Id, actor.FirstName, actor.LastName),
            notification.TicketId,
            notification.Type,
            notification.Title,
            notification.Message,
            notification.IsRead,
            notification.CreatedAtUtc,
            notification.ReadAtUtc);
    }
}
