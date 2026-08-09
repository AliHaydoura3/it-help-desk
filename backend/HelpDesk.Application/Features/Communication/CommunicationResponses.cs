using HelpDesk.Domain;

namespace HelpDesk.Application.Features.Communication;

public sealed record CommentAuthorResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email);

public sealed record MentionedAgentResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email);

public sealed record TicketCommentResponse(
    Guid Id,
    Guid TicketId,
    Guid? ParentCommentId,
    string Content,
    CommentAuthorResponse Author,
    IReadOnlyList<MentionedAgentResponse> Mentions,
    DateTime CreatedAtUtc);

public sealed record GetTicketCommentsResponse(
    IReadOnlyList<TicketCommentResponse> Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages);

public sealed record MentionableAgentResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email);

public sealed record NotificationActorResponse(
    Guid Id,
    string FirstName,
    string LastName);

public sealed record NotificationResponse(
    Guid Id,
    NotificationActorResponse Actor,
    Guid? TicketId,
    NotificationType Type,
    string Title,
    string Message,
    bool IsRead,
    DateTime CreatedAtUtc,
    DateTime? ReadAtUtc);

public sealed record GetNotificationsResponse(
    IReadOnlyList<NotificationResponse> Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages,
    int UnreadCount);

public sealed record UnreadNotificationCountResponse(int UnreadCount);

public sealed record MarkAllNotificationsReadResponse(int MarkedCount);
