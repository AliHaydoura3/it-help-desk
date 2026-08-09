namespace HelpDesk.Domain;

public enum NotificationType
{
    TicketCreated,
    TicketUpdated,
    TicketStatusChanged,
    TicketCancelled,
    TicketAssigned,
    TicketReassigned,
    TicketEscalated,
    CommentAdded,
    ReplyAdded,
    AgentMentioned,
    InternalNoteAdded
}

public enum NotificationEmailStatus
{
    Pending,
    Sent,
    Failed,
    Skipped
}

public sealed class Notification
{
    private Notification() { }

    public Guid Id { get; private set; }
    public Guid RecipientUserId { get; private set; }
    public Guid ActorUserId { get; private set; }
    public Guid? TicketId { get; private set; }
    public NotificationType Type { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public bool IsRead { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? ReadAtUtc { get; private set; }
    public DateTime? RealtimeDeliveredAtUtc { get; private set; }
    public int RealtimeAttempts { get; private set; }
    public string? LastRealtimeError { get; private set; }
    public NotificationEmailStatus EmailStatus { get; private set; }
    public int EmailAttempts { get; private set; }
    public DateTime? EmailSentAtUtc { get; private set; }
    public string? LastEmailError { get; private set; }

    public static Notification Create(
        Guid recipientUserId,
        Guid actorUserId,
        Guid? ticketId,
        NotificationType type,
        string title,
        string message,
        DateTime createdAtUtc,
        bool sendEmail = true)
    {
        if (recipientUserId == Guid.Empty)
            throw new DomainRuleException("A notification recipient is required.");
        if (actorUserId == Guid.Empty)
            throw new DomainRuleException("A notification actor is required.");
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainRuleException("A notification title is required.");
        if (string.IsNullOrWhiteSpace(message))
            throw new DomainRuleException("A notification message is required.");

        return new Notification
        {
            Id = Guid.NewGuid(),
            RecipientUserId = recipientUserId,
            ActorUserId = actorUserId,
            TicketId = ticketId,
            Type = type,
            Title = title.Trim(),
            Message = message.Trim(),
            CreatedAtUtc = createdAtUtc,
            EmailStatus = sendEmail
                ? NotificationEmailStatus.Pending
                : NotificationEmailStatus.Skipped
        };
    }

    public void MarkRead(DateTime readAtUtc)
    {
        if (IsRead) return;
        IsRead = true;
        ReadAtUtc = readAtUtc;
    }

    public void MarkRealtimeDelivered(DateTime deliveredAtUtc)
    {
        RealtimeAttempts++;
        RealtimeDeliveredAtUtc ??= deliveredAtUtc;
        LastRealtimeError = null;
    }

    public void MarkRealtimeFailed(string error)
    {
        RealtimeAttempts++;
        LastRealtimeError = error.Length <= 1000 ? error : error[..1000];
    }

    public void MarkEmailSent(DateTime sentAtUtc)
    {
        EmailAttempts++;
        EmailStatus = NotificationEmailStatus.Sent;
        EmailSentAtUtc = sentAtUtc;
        LastEmailError = null;
    }

    public void MarkEmailFailed(string error)
    {
        EmailAttempts++;
        EmailStatus = NotificationEmailStatus.Failed;
        LastEmailError = error.Length <= 1000 ? error : error[..1000];
    }

    public void SkipEmail(string reason)
    {
        EmailStatus = NotificationEmailStatus.Skipped;
        LastEmailError = reason.Length <= 1000 ? reason : reason[..1000];
    }
}
