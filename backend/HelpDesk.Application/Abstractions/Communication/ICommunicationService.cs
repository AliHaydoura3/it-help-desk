using HelpDesk.Application.Features.Communication;
using HelpDesk.Application.Features.Communication.Comments;
using HelpDesk.Application.Features.Communication.Mentions;
using HelpDesk.Application.Features.Communication.Notifications;

namespace HelpDesk.Application.Abstractions.Communication;

public interface ICommunicationService
{
    Task<TicketCommentResponse> AddCommentAsync(AddTicketCommentCommand command, CancellationToken cancellationToken);
    Task<GetTicketCommentsResponse> GetCommentsAsync(GetTicketCommentsQuery query, CancellationToken cancellationToken);
    Task<IReadOnlyList<MentionableAgentResponse>> GetMentionableAgentsAsync(GetMentionableAgentsQuery query, CancellationToken cancellationToken);
    Task<GetNotificationsResponse> GetNotificationsAsync(GetNotificationsQuery query, CancellationToken cancellationToken);
    Task<UnreadNotificationCountResponse> GetUnreadCountAsync(GetUnreadNotificationCountQuery query, CancellationToken cancellationToken);
    Task<NotificationResponse> MarkNotificationReadAsync(MarkNotificationReadCommand command, CancellationToken cancellationToken);
    Task<MarkAllNotificationsReadResponse> MarkAllNotificationsReadAsync(MarkAllNotificationsReadCommand command, CancellationToken cancellationToken);
}
