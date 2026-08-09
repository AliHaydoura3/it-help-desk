namespace HelpDesk.Domain;

public enum TicketCategory { Hardware, Software, Network, Email, AccessRequest, Other }
public enum TicketPriority { Low, Medium, High, Critical }
public enum TicketStatus { Open, InProgress, Pending, Resolved, Closed }
public enum TicketAssignmentType { Manual, Automatic, Reassignment }
public enum TicketEscalationLevel { None, Level1, Level2, Level3 }

public sealed class Ticket
{
    private readonly List<TicketHistory> _history = [];
    private readonly List<TicketAssignmentHistory> _assignmentHistory = [];
    private readonly List<TicketInternalNote> _internalNotes = [];
    private readonly List<TicketAttachment> _attachments = [];

    private Ticket() { }

    public Guid Id { get; private set; }
    public string ReferenceNumber { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public TicketCategory Category { get; private set; }
    public TicketPriority Priority { get; private set; }
    public TicketStatus Status { get; private set; }
    public TicketEscalationLevel EscalationLevel { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public Guid? AssignedToUserId { get; private set; }
    public bool IsCancelled { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public DateTime? ResolvedAtUtc { get; private set; }
    public DateTime? ClosedAtUtc { get; private set; }
    public byte[] RowVersion { get; private set; } = [];
    public IReadOnlyCollection<TicketHistory> History => _history;
    public IReadOnlyCollection<TicketAssignmentHistory> AssignmentHistory => _assignmentHistory;
    public IReadOnlyCollection<TicketInternalNote> InternalNotes => _internalNotes;
    public IReadOnlyCollection<TicketAttachment> Attachments => _attachments;

    public static Ticket Create(Guid creatorId, string reference, string title, string description,
        TicketCategory category, TicketPriority priority, DateTime occurredAtUtc)
    {
        var ticket = new Ticket
        {
            Id = Guid.NewGuid(), ReferenceNumber = reference, Title = title.Trim(),
            Description = description.Trim(), Category = category, Priority = priority,
            Status = TicketStatus.Open, CreatedByUserId = creatorId,
            CreatedAtUtc = occurredAtUtc, UpdatedAtUtc = occurredAtUtc
        };
        ticket.RecordHistory(creatorId, "Created", null, TicketStatus.Open.ToString(), occurredAtUtc);
        return ticket;
    }

    public void UpdateDetails(Guid actorId, string title, string description, TicketCategory category,
        TicketPriority priority, DateTime occurredAtUtc)
    {
        EnsureActive();
        var previous = $"{Title} | {Category} | {Priority}";
        Title = title.Trim(); Description = description.Trim(); Category = category; Priority = priority;
        Touch(occurredAtUtc);
        RecordHistory(actorId, "Updated", previous, $"{Title} | {Category} | {Priority}", occurredAtUtc);
    }

    public void ChangeStatus(Guid actorId, TicketStatus status, DateTime occurredAtUtc)
    {
        EnsureActive();
        if (Status == status) return;
        var previous = Status;
        Status = status;
        if (status == TicketStatus.Resolved)
            ResolvedAtUtc ??= occurredAtUtc;
        if (status == TicketStatus.Closed)
        {
            ResolvedAtUtc ??= occurredAtUtc;
            ClosedAtUtc ??= occurredAtUtc;
        }
        Touch(occurredAtUtc);
        RecordHistory(actorId, "Status changed", previous.ToString(), status.ToString(), occurredAtUtc);
    }

    public void Assign(Guid actorId, Guid agentId, TicketAssignmentType assignmentType, DateTime occurredAtUtc)
    {
        EnsureActive();
        EnsureWorkflowOpen();
        if (AssignedToUserId == agentId) throw new DomainRuleException("The ticket is already assigned to this agent.");
        var previousAgent = AssignedToUserId;
        AssignedToUserId = agentId;
        var effectiveType = previousAgent.HasValue ? TicketAssignmentType.Reassignment : assignmentType;
        _assignmentHistory.Add(TicketAssignmentHistory.Create(Id, actorId, previousAgent, agentId, effectiveType, occurredAtUtc));
        RecordHistory(actorId, effectiveType == TicketAssignmentType.Reassignment ? "Reassigned" : "Assigned",
            previousAgent?.ToString(), agentId.ToString(), occurredAtUtc);
        Touch(occurredAtUtc);
    }

    public void Escalate(Guid actorId, TicketEscalationLevel level, string reason, DateTime occurredAtUtc)
    {
        EnsureActive();
        EnsureWorkflowOpen();
        if (level <= EscalationLevel) throw new DomainRuleException("An escalation must move the ticket to a higher level.");
        var previous = EscalationLevel; EscalationLevel = level;
        if (level >= TicketEscalationLevel.Level2 && Priority < TicketPriority.High) Priority = TicketPriority.High;
        if (level == TicketEscalationLevel.Level3) Priority = TicketPriority.Critical;
        RecordHistory(actorId, "Escalated", previous.ToString(), $"{level}: {reason.Trim()}", occurredAtUtc);
        Touch(occurredAtUtc);
    }

    public TicketInternalNote AddInternalNote(Guid authorId, string content, DateTime occurredAtUtc)
    {
        EnsureActive();
        var note = TicketInternalNote.Create(Id, authorId, content.Trim(), occurredAtUtc);
        _internalNotes.Add(note);
        RecordHistory(authorId, "Internal note added", null, note.Id.ToString(), occurredAtUtc);
        Touch(occurredAtUtc);
        return note;
    }

    public TicketHistory RecordComment(
        Guid actorId,
        Guid commentId,
        bool isReply,
        DateTime occurredAtUtc)
    {
        EnsureActive();
        if (Status == TicketStatus.Closed)
            throw new DomainRuleException("A closed ticket cannot receive new comments.");

        Touch(occurredAtUtc);
        RecordHistory(
            actorId,
            isReply ? "Reply added" : "Comment added",
            null,
            commentId.ToString(),
            occurredAtUtc);

        return _history[^1];
    }

    public TicketAttachment AddAttachment(
        Guid uploaderUserId,
        string originalFileName,
        string storageKey,
        string contentType,
        string extension,
        long sizeBytes,
        string sha256Hash,
        DateTime occurredAtUtc)
    {
        EnsureActive();
        if (Status == TicketStatus.Closed)
            throw new DomainRuleException("A closed ticket cannot receive new attachments.");

        var attachment = TicketAttachment.Create(
            Id,
            uploaderUserId,
            originalFileName,
            storageKey,
            contentType,
            extension,
            sizeBytes,
            sha256Hash,
            occurredAtUtc);
        _attachments.Add(attachment);
        Touch(occurredAtUtc);
        RecordHistory(
            uploaderUserId,
            "Attachment uploaded",
            null,
            $"{attachment.Id}: {attachment.OriginalFileName}",
            occurredAtUtc);
        return attachment;
    }

    public void Cancel(Guid actorId, DateTime occurredAtUtc)
    {
        EnsureActive(); IsCancelled = true; Status = TicketStatus.Closed;
        ClosedAtUtc ??= occurredAtUtc; Touch(occurredAtUtc);
        RecordHistory(actorId, "Cancelled", null, TicketStatus.Closed.ToString(), occurredAtUtc);
    }

    private void EnsureActive() { if (IsCancelled) throw new DomainRuleException("A cancelled ticket cannot be modified."); }
    private void EnsureWorkflowOpen()
    {
        if (Status is TicketStatus.Resolved or TicketStatus.Closed)
            throw new DomainRuleException("Resolved or closed tickets cannot be assigned or escalated.");
    }
    private void Touch(DateTime occurredAtUtc) => UpdatedAtUtc = occurredAtUtc;
    private void RecordHistory(Guid actorId, string action, string? previous, string? next, DateTime occurredAtUtc) =>
        _history.Add(TicketHistory.Create(Id, actorId, action, previous, next, occurredAtUtc));
}

public sealed class TicketHistory
{
    private TicketHistory() { }
    public Guid Id { get; private set; }
    public Guid TicketId { get; private set; }
    public Guid ActorUserId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string? PreviousValue { get; private set; }
    public string? NewValue { get; private set; }
    public DateTime OccurredAtUtc { get; private set; }
    internal static TicketHistory Create(Guid ticketId, Guid actorId, string action, string? previous, string? next, DateTime at) =>
        new() { Id = Guid.NewGuid(), TicketId = ticketId, ActorUserId = actorId, Action = action, PreviousValue = previous, NewValue = next, OccurredAtUtc = at };
}

public sealed class TicketAssignmentHistory
{
    private TicketAssignmentHistory() { }
    public Guid Id { get; private set; }
    public Guid TicketId { get; private set; }
    public Guid ActorUserId { get; private set; }
    public Guid? PreviousAgentId { get; private set; }
    public Guid AssignedAgentId { get; private set; }
    public TicketAssignmentType AssignmentType { get; private set; }
    public DateTime OccurredAtUtc { get; private set; }
    internal static TicketAssignmentHistory Create(Guid ticketId, Guid actorId, Guid? previous, Guid agent, TicketAssignmentType type, DateTime at) =>
        new() { Id = Guid.NewGuid(), TicketId = ticketId, ActorUserId = actorId, PreviousAgentId = previous, AssignedAgentId = agent, AssignmentType = type, OccurredAtUtc = at };
}

public sealed class TicketInternalNote
{
    private TicketInternalNote() { }
    public Guid Id { get; private set; }
    public Guid TicketId { get; private set; }
    public Guid AuthorUserId { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public DateTime CreatedAtUtc { get; private set; }
    internal static TicketInternalNote Create(Guid ticketId, Guid authorId, string content, DateTime at) =>
        new() { Id = Guid.NewGuid(), TicketId = ticketId, AuthorUserId = authorId, Content = content, CreatedAtUtc = at };
}

public sealed class DomainRuleException(string message) : Exception(message);
